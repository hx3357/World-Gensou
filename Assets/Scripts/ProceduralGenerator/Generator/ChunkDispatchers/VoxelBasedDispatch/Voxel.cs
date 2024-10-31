using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.VirtualTexturing;

namespace ChunkDispatchers.VoxelBasedDispatch
{
    public class Voxel
    {
        
        public Vector3 worldOrigin;
        // Chunk count
         readonly int size;
        public readonly float worldSize;
        public readonly Vector3 center;
    
        //The number of sampled dots in the voxel
        public int dotCount{ get; private set;}
        
        private Voxel[] childVoxels;
        private Voxel fatherVoxel;
        
        const int MAX_DEPTH = 3;
        
        bool isGenerate = false;

        public int voxelIndice;
        // This type is depend on the exact use case defined in the on generate action
        public int voxelType;

        private bool isLeaf => childVoxels == null && isGenerate;
        public readonly bool isRoot;
        
        //public Dictionary<Vector3Int,ChunkParameter> cachedChunkCoordMap;
        private bool isCalculatedChunkCoordMap = false;

        private readonly float subVoxelExpandFactor;
        
        public int chunkResolution = 31;
        
        public bool isUpdateVoxel = false;
        
        private Voxel rootVoxel;

        public int depth;
        
        public Voxel(Vector3 m_worldOrigin, float m_worldSize,int m_dotCountExpection, bool m_isRoot,Voxel m_rootVoxel,
            int depth = 0,float initDotExp = 1f,Voxel m_fatherVoxel = null,int m_voxelIndice = 0,
            float mainVoxelshrinkFactor = 0.7f,float p_MainVoxel=0.6f, float _subVoxelExpandFactor = 0f
            , Action<Voxel> onGenerate = null)
        {
            size = Mathf.RoundToInt(m_worldSize/Chunk.GetWorldSize()[0]);
            worldOrigin = m_worldOrigin;
            worldSize = m_worldSize ;
            center = m_worldOrigin + worldSize / 2 * Vector3.one;
            isRoot = m_isRoot;
            rootVoxel = isRoot ? this : m_rootVoxel;
            subVoxelExpandFactor = _subVoxelExpandFactor;
            fatherVoxel = m_fatherVoxel;
            voxelIndice = m_voxelIndice;
            this.depth = depth;
            
            dotCount = m_isRoot ? PoissonSampler.GetPoissonSampleCount(initDotExp, center) : 
                m_dotCountExpection;
            
            if (dotCount > 1)
            {
                Spilt(depth,dotCount,onGenerate);
            }
            else if (dotCount == 1)
            {
                if(Mathf.Abs(m_worldOrigin.GetHashCode() % 10000 / 10000f)>p_MainVoxel)
                    Spilt(depth,dotCount+1,onGenerate);
                else
                {
                    // Generate Leaf voxel
                    isGenerate = true;
                    if (m_isRoot)
                    {
                        worldOrigin += HashUtility.Get3DHash(worldOrigin) * (worldSize * (1-mainVoxelshrinkFactor));
                        worldSize *= mainVoxelshrinkFactor;
                        center = worldOrigin + worldSize / 2 * Vector3.one;
                    }
                    onGenerate?.Invoke(this);
                }
            }
        }
        

        void Spilt(int curDepth,float dotCountExp,Action<Voxel> onGenerate)
        {
            if(curDepth >= MAX_DEPTH)
                return;
           
        
            int[] subVoxelDotCountList = new int[8];
            Vector3[] newOriginList = new Vector3[8];
            bool[] isFilledList = new bool[8];
            int subVoxelBitmap = 0;
            int subVoxelCount = 0;
        
            for (int i = 0; i < 8; i++)
            {
                Vector3 newOrigin = worldOrigin + new Vector3(i & 1, (i & 2) >> 1, (i & 4) >> 2) * worldSize / 2;
                int subVoxelDotCount = 
                    PoissonSampler.GetPoissonSampleCount(dotCountExp/8, newOrigin + Vector3.one * worldSize / 4);
                newOriginList[i] = newOrigin;
                subVoxelDotCountList[i] = subVoxelDotCount;
                if (subVoxelDotCount != 0)
                {
                    subVoxelBitmap |= 1 << i;
                    subVoxelCount++;
                    isFilledList[i] = true;
                }
            }
            
            if (subVoxelCount > 1)
            {
                // Displacement
                
                int[][] displacements = VoxelDisplacementTable.Table[subVoxelBitmap];
                int[] displacement = displacements[Mathf.Abs((center*0.127f + 57 * Vector3.one).GetHashCode()) % displacements.Length];
                
                // Key: The block indice to be displaced
                // Value: The position list which determines where the voxel can be displaced
                Dictionary<int,List <Vector3>> subVoxelDisplacedCentersDict = new ();
                
                for (int i = 0; i < displacement.Length; i++)
                {
                    if (!subVoxelDisplacedCentersDict.ContainsKey(displacement[i]))
                    {
                        subVoxelDisplacedCentersDict[displacement[i]] = new List<Vector3>();
                    }
                    
                    subVoxelDisplacedCentersDict[displacement[i]].Add(newOriginList[i]);
                }

                Vector3[] finalDisplacementCenterMap = new Vector3[8];

                foreach (var kvPair in subVoxelDisplacedCentersDict)
                {
                    Vector3 displacementAreaOrigin = int.MaxValue * Vector3.one;
                    Vector3 displacementAreaEnd = -displacementAreaOrigin;
                    foreach (var voxelOrigin in kvPair.Value)
                    {
                        if (voxelOrigin.x <= displacementAreaOrigin.x &&
                            voxelOrigin.y <= displacementAreaOrigin.y &&
                            voxelOrigin.z <= displacementAreaOrigin.z)
                        {
                            displacementAreaOrigin = voxelOrigin;
                        }
                        if (voxelOrigin.x >= displacementAreaEnd.x &&
                            voxelOrigin.y >= displacementAreaEnd.y &&
                            voxelOrigin.z >= displacementAreaEnd.z)
                        {
                            displacementAreaEnd = voxelOrigin;
                        }
                    }
                    
                    finalDisplacementCenterMap[kvPair.Key] = displacementAreaOrigin + 
                                                       Vector3.Scale(HashUtility.Get3DHash(newOriginList[kvPair.Key]), 
                                                           displacementAreaEnd - displacementAreaOrigin);
                }
                
                childVoxels = new Voxel[8];
                
                for (int i = 0; i < 8; i++)
                {
                    if (!isFilledList[i])
                        continue;
                    Vector3 finalOrigin = finalDisplacementCenterMap[i];
                    float finalSize = worldSize / 2;
                    
                    //Make the sub voxel close together
                    finalSize *= 1 + subVoxelExpandFactor;
                    finalOrigin += (worldOrigin - finalOrigin).normalized * ((finalSize - worldSize/2)/2 * 1.42f * 1.2f);

                    childVoxels[i] = new Voxel(finalOrigin, finalSize, subVoxelDotCountList[i],
                        false,rootVoxel ,curDepth + 1,
                        m_fatherVoxel:this,m_voxelIndice:i,
                        onGenerate: onGenerate);
                }
                
            }
            else if(subVoxelCount == 1)
            {
                childVoxels = new Voxel[8];
                for(int i = 0; i < 8; i++)
                {
                    if (!isFilledList[i])
                        continue;
                    childVoxels[i] = new Voxel(worldOrigin + HashUtility.Get3DHash(newOriginList[i]) * worldSize / 4,
                        worldSize / 2, subVoxelDotCountList[i], false, rootVoxel,curDepth + 1,
                        m_fatherVoxel: this,
                        onGenerate: onGenerate);
                }
            }
        }
        

        public void CalculateChunkCoords(Dictionary<Vector3Int,ChunkParameter> chunkCoordMap,bool isForce = false)
        {
            if(isCalculatedChunkCoordMap && !isForce)
                return;
            
            isCalculatedChunkCoordMap = true;
            
            if (isLeaf)
            {
                Vector3Int[] chunkCoords = GetChunkCoords();
                foreach (var chunkCoord in chunkCoords)
                {
                    chunkCoordMap.TryAdd(chunkCoord, new ChunkParameter());
                    chunkCoordMap[chunkCoord].Add(this);
                    chunkCoordMap[chunkCoord].rootVoxel = rootVoxel;
                }
            }
            else
            {
                if (childVoxels == null) return;
                foreach (var childVoxel in childVoxels)
                {
                    childVoxel?.CalculateChunkCoords(chunkCoordMap);
                }
            }
        }

        public void RemoveChunkCoords(Dictionary<Vector3Int, ChunkParameter> chunkCoordMap)
        {
            Dictionary<Vector3Int, ChunkParameter> curChunkCoordMap = new();
            CalculateChunkCoords(curChunkCoordMap,true);
            foreach (var chunkCoord in curChunkCoordMap.Keys)
            {
                chunkCoordMap.Remove(chunkCoord);
            }
        }

        public float GetMinDistance(Vector3 pos)
        {
            Vector3 closestPoint = Vector3.zero;
            closestPoint.x = Mathf.Clamp(pos.x, worldOrigin.x, worldOrigin.x + worldSize);
            closestPoint.y = Mathf.Clamp(pos.y, worldOrigin.y, worldOrigin.y + worldSize);
            closestPoint.z = Mathf.Clamp(pos.z, worldOrigin.z, worldOrigin.z + worldSize);
            return Vector3.Distance(pos, closestPoint);
        }
        
        public float GetMaxDistance(Vector3 pos)
        {
            Vector3[] corners = new Vector3[8];
            for(int i = 0; i < 8; i++)
            {
                corners[i] = worldOrigin + new Vector3(i & 1, (i & 2) >> 1, (i & 4) >> 2) * worldSize;
            }
            float maxDistance = 0;
            foreach (var corner in corners)
            {
                float distance = Vector3.Distance(corner, pos);
                if (distance > maxDistance)
                    maxDistance = distance;
            }
            return maxDistance;
        }

        Vector3Int ConvertToChunkCoord()
        {
            return new Vector3Int(Mathf.RoundToInt(worldOrigin.x/Chunk.GetWorldSize()[0]),
                Mathf.RoundToInt(worldOrigin.y/Chunk.GetWorldSize()[1]),
                Mathf.RoundToInt(worldOrigin.z/Chunk.GetWorldSize()[2]));
        }
        
        public Vector3Int[] GetChunkCoords()
        {
            Vector3 voxelOrigin = worldOrigin;
            Vector3 voxelEnd = worldOrigin + Vector3.one * worldSize;
            Vector3Int startChunkCoord = Chunk.GetChunkCoordByPosition(voxelOrigin);
            Vector3Int endChunkCoord = Chunk.GetChunkCoordByPosition(voxelEnd);
            if (Vector3.Distance(voxelEnd, Chunk.GetChunkOriginByCoord(endChunkCoord)) < 0.01)
            {
                endChunkCoord -= Vector3Int.one;
            }
            List<Vector3Int> chunkCoords = new List<Vector3Int>();
            for (int x = startChunkCoord.x; x <= endChunkCoord.x; x++)
                for(int y = startChunkCoord.y; y <= endChunkCoord.y; y++)
                    for(int z = startChunkCoord.z; z <= endChunkCoord.z; z++)
                    {
                        chunkCoords.Add(new Vector3Int(x,y,z));
                    }
            return chunkCoords.ToArray();
        }

        public Voxel[] GetLeafVoxels()
        {
            if (isLeaf)
            {
                return new []{this};
            }
            List<Voxel> leafVoxels = new List<Voxel>();
            foreach (var childVoxel in childVoxels)
            {
                leafVoxels.AddRange(childVoxel.GetLeafVoxels());
            }
            return leafVoxels.ToArray();
        }
        
        
    
        public void DrawLeafGizmos()
        {
            if (isLeaf)
            {
                Gizmos.color = new Color(0.5f,0,1,0.2f);
                Gizmos.DrawCube(center, Vector3.one * worldSize);
                //Gizmos.DrawCube(center, 10*Vector3.one);
                // Gizmos.color = new Color(0.9f,0,1,0.5f);
                // foreach (var chunk in GetChunkCoords())
                // {
                //     Chunk.DrawChunkGizmo(chunk);
                // }
                //
            }
            else
            {
                if(childVoxels != null)
                    foreach (var childVoxel in childVoxels)
                    {
                        childVoxel?.DrawLeafGizmos();
                    }
            }
        }
    
        public float GetLeafVolume()
        {
            if (childVoxels == null)
            {
                if (dotCount != 0) return worldSize * worldSize * worldSize;
                
                return 0;
            }
            float volume = 0;
            foreach (var childVoxel in childVoxels)
            {
                if(childVoxel == null)
                    continue;
                volume += childVoxel.GetLeafVolume();
            }
            return volume;
        }
    
        public void DrawGizmo()
        {
            Gizmos.color = isUpdateVoxel? Color.blue : Color.red;
            Gizmos.DrawWireCube(center, Vector3.one * worldSize);
            Handles.Label(center, chunkResolution.ToString());
        }
    }
}
