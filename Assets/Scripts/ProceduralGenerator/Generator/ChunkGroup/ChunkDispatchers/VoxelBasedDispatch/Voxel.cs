using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChunkDispatchers.VoxelBasedDispatch
{
    public class Voxel
    {
        
        readonly Vector3 worldOrigin;
        // Chunk count
        private readonly int size;
        readonly float worldSize;
        readonly Vector3 center;
    
        //The number of sampled dots in the voxel
        public int dotCount{ get; private set;}
        
        private Voxel[] childVoxels;
        private Voxel fatherVoxel;
        
        const int MAX_DEPTH = 3;
        
        bool isGenerate = false;

        private int voxelIndice;
        private int voxelType;
        
        bool isLeaf => childVoxels == null && isGenerate;
        private bool isRoot;
        
        public Dictionary<Vector3Int,ChunkParameter> cachedChunkCoordMap;

        private readonly float subVoxelExpandFactor;
        
        public Voxel(Vector3 m_worldOrigin, float m_worldSize,int m_dotCountExpection, bool isRoot,
            int depth = 0,float initDotExp = 1f,int indice = 0,
            float mainVoxelshrinkFactor = 0.9f,float p_MainVoxel=0.6f, float _subVoxelExpandFactor = 0.5f,float lake_P = 0.4f)
        {
            size = Mathf.RoundToInt(m_worldSize/Chunk.GetWorldSize()[0]);
            worldOrigin = m_worldOrigin;
            worldSize = m_worldSize ;
            center = m_worldOrigin + worldSize / 2 * Vector3.one;
            voxelIndice = indice;
            subVoxelExpandFactor = _subVoxelExpandFactor;
            
            dotCount = isRoot ? PoissonSampler.GetPoissonSampleCount(initDotExp, center) : 
                m_dotCountExpection;
            
            if (dotCount > 1)
            {
                Spilt(depth,dotCount+1);
            }
            else if (dotCount == 1)
            {
                if(Mathf.Abs(m_worldOrigin.GetHashCode() % 10000 / 10000f)>p_MainVoxel)
                    Spilt(depth,dotCount+1);
                else
                {
                    isGenerate = true;
                    if (isRoot)
                    {
                        if (Mathf.Abs(m_worldOrigin.GetHashCode() % 10000 / 10000f) < lake_P)
                        {
                            // Generate lake island
                            voxelType = 2;
                            
                        }
                        worldOrigin += HashUtility.Get3DHash(worldOrigin) * (worldSize * (1-mainVoxelshrinkFactor));
                        worldSize *= mainVoxelshrinkFactor;
                        center = worldOrigin + worldSize / 2 * Vector3.one;
                    }
                }
            }
        }

        void Spilt(int curDepth,float dotCountExp)
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
                        false,curDepth+1,indice:i)
                        {
                            fatherVoxel = this,
                            voxelType = ((i & 3) >> 2) == 1 ? 0 : 1
                        };
                }
                
            }
            else if(subVoxelCount == 1)
            {
                childVoxels = new Voxel[8];
                for(int i = 0; i < 8; i++)
                {
                    if (!isFilledList[i])
                        continue;
                    childVoxels[i] = new Voxel(worldOrigin + HashUtility.Get3DHash(newOriginList[i]) * worldSize/4, 
                        worldSize/2, subVoxelDotCountList[i],false,curDepth+1)
                    {
                        fatherVoxel = this
                    };
                }
            }
        }
        

        public Dictionary<Vector3Int,ChunkParameter>  CalculateChunkCoords()
        {
            if (isLeaf)
            {
                Vector3Int[] chunkCoords = GetChunkCoords();
                Dictionary<Vector3Int,ChunkParameter> chunkCoordMap = new();
                foreach (var chunkCoord in chunkCoords)
                {
                    chunkCoordMap.TryAdd(chunkCoord, new ChunkParameter());
                    chunkCoordMap[chunkCoord].Add(center,worldSize,voxelType);
                }
                return chunkCoordMap;
            }
            else
            {
                if (childVoxels == null) return null;
                Dictionary<Vector3Int,ChunkParameter> chunkCoordMap = new();
                foreach (var childVoxel in childVoxels)
                {
                    var childChunkCoordMap = childVoxel?.CalculateChunkCoords();
                    if(childChunkCoordMap == null)
                        continue;
                    foreach (var coord in childChunkCoordMap.Keys)
                    {
                        if (chunkCoordMap.ContainsKey(coord))
                        {
                            chunkCoordMap[coord].Merge(childChunkCoordMap[coord]);
                        }
                        else
                        {
                            chunkCoordMap.Add(coord, childChunkCoordMap[coord]);
                        }
                    }
                }
                cachedChunkCoordMap = chunkCoordMap;
                return chunkCoordMap;
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
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(center, Vector3.one * worldSize);
        }
    }
}
