using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace ChunkDispatchers.VoxelBasedDispatch
{
    public class ChunkParameter
    {
        List<Vector3> voxelPositions = new();
        List<float> voxelSize = new();
        
        public void Add(Vector3 position, float size)
        {
            voxelPositions.Add(position);
            voxelSize.Add(size);
        }
        
        public void Merge(ChunkParameter chunkParameter)
        {
            voxelPositions.AddRange(chunkParameter.voxelPositions);
            voxelSize.AddRange(chunkParameter.voxelSize);
        }
        
        public SDFIslandSFGParameter ToIslandSFGParameter()
        {
            Vector4[] islandPositions = new Vector4[voxelPositions.Count];
            Vector4[] islandParameters = new Vector4[voxelPositions.Count];
            for (int i = 0; i < voxelPositions.Count; i++)
            {
                islandPositions[i] = new Vector4(voxelPositions[i].x,voxelPositions[i].y,voxelPositions[i].z,0);
                islandParameters[i] = new Vector4(voxelSize[i]/2,voxelSize[i]/2,0,0);
            }

            return new SDFIslandSFGParameter(islandPositions,islandParameters);
        }
    }
    
    /// <summary>
    /// Designed for sdf island SFG
    /// 
    /// Suitable for surrounding boxs which size varies greatly.
    /// When the dot count is greater than 1, the voxel will be cut into 8 sub voxels and distribute its dot count to the sub voxels.
    /// When the dot count is 1, the chunk will be generated inside the voxel.
    /// When the dot count is 0, the voxel will be ignored.
    /// Surrounding boxs whose size is close to each other will be more likely to stay together.
    /// </summary>
    public class VoxelBasedRandomPointDispatcher : IChunkDispatcher
    {
        readonly VoxelMap baseVoxelMap;
       // readonly VoxelMap[] voxelMaps;
        readonly float dotCountExpection;

        // Key: voxel coord
        private Dictionary<Vector3Int, Voxel> baseVoxelDictionary = new();
        private Vector3Int lastPlayerVoxelCoord ;
        private bool isFirstTime = true;
        private Vector3 playerPos;
        private float maxViewDistance;
        
    
        public VoxelBasedRandomPointDispatcher(VoxelMap[] m_voxelMaps,float baseVoxelSizeOffset, float dotCountExpection =1f)
        {
            // this.voxelMaps =new VoxelMap[m_voxelMaps.Length];
            // Array.Copy(m_voxelMaps,this.voxelMaps,m_voxelMaps.Length);
            // Array.Sort(voxelMaps,(a,b)=>a.voxelSize.CompareTo(b.voxelSize));
            this.dotCountExpection = dotCountExpection;
           // baseVoxelMap = new VoxelMap(m_voxelMaps[^1].voxelSize+baseVoxelSizeOffset);
           baseVoxelMap = new VoxelMap(Chunk.GetWorldSize()[0]*10);
        }
  
    
        public void DispatchChunks(SurroundBox chunkGroupSurroundBox,HashSet<Vector3Int> activeChunks, Vector3 playerPosition, float maxViewDistance, 
            out List<Vector3Int> chunksToGenerate, out List<Vector3Int> chunksToDestroy, out List<object> chunkParameters)
        {
            chunksToGenerate = new ();
            chunksToDestroy = new ();
            chunkParameters = new();
        
            Vector3Int _playerVoxelCoord = baseVoxelMap.GetVoxelCoordByPosition(playerPosition);
            int celledMaxViewedVoxelRadius = Mathf.CeilToInt(maxViewDistance*Chunk.GetWorldSize()[0]/baseVoxelMap.voxelSize) ;
            
            List<Voxel> voxelsToGenerate = new();
            playerPos = playerPosition;
            this.maxViewDistance= maxViewDistance;
            
            if(isFirstTime || _playerVoxelCoord != lastPlayerVoxelCoord)
            {
                for (int x = -celledMaxViewedVoxelRadius; x <= celledMaxViewedVoxelRadius; x++)
                for (int y = -celledMaxViewedVoxelRadius / 2; y <= celledMaxViewedVoxelRadius / 2; y++)
                for (int z = -celledMaxViewedVoxelRadius; z <= celledMaxViewedVoxelRadius; z++)
                {
                    Vector3Int voxelCoord = _playerVoxelCoord + new Vector3Int(x, y, z);
                    Vector3 voxelOrigin = baseVoxelMap.GetVoxelOriginByCoord(voxelCoord);
                    float distance = Vector3Int.Distance(voxelCoord, _playerVoxelCoord);
                    if (distance < celledMaxViewedVoxelRadius)
                    {
                        baseVoxelDictionary.TryAdd(voxelCoord,new Voxel(voxelOrigin, baseVoxelMap.voxelSize, 0,
                            true, initDotExp: dotCountExpection));
                    }
                }
                
                lastPlayerVoxelCoord = _playerVoxelCoord;
            
                foreach (var voxelCoord in baseVoxelDictionary.Keys.ToArray())
                {
                    float distance = Vector3Int.Distance(voxelCoord,_playerVoxelCoord);
                    if(distance>celledMaxViewedVoxelRadius)
                    {
                        baseVoxelDictionary.Remove(voxelCoord);
                    }
                }
            }

            foreach (var voxel in baseVoxelDictionary.Values)
            {
                if (!isFirstTime&&
                    (voxel.GetMinDistance(playerPosition) > maxViewDistance * Chunk.GetWorldSize()[0] ||
                     voxel.GetMaxDistance(playerPosition) < maxViewDistance * Chunk.GetWorldSize()[0]))
                {
                    Debug.Log("skipped");
                    continue;
                }
                var chunkCoordsMap =voxel.CalculateChunkCoords();
                if(chunkCoordsMap!=null)
                    foreach (var chunkCoord in chunkCoordsMap)
                    {
                        if(!activeChunks.Contains(chunkCoord.Key)&&
                           Vector3.Distance(Chunk.GetChunkCenterByCoord(chunkCoord.Key),playerPosition)<maxViewDistance*Chunk.GetWorldSize()[0])
                        {
                            chunksToGenerate.Add(chunkCoord.Key);
                            chunkParameters.Add(chunkCoord.Value.ToIslandSFGParameter());
                        }

                        if (activeChunks.Contains(chunkCoord.Key) &&
                            Vector3.Distance(Chunk.GetChunkCenterByCoord(chunkCoord.Key), playerPosition) >
                            maxViewDistance * Chunk.GetWorldSize()[0])
                        {
                            chunksToDestroy.Add(chunkCoord.Key);
                        }
                    }
            }
            
            isFirstTime = false;

            if (IChunkDispatcher.isDebug)
            {
                float volumeSum = 0;
                foreach (var voxel in baseVoxelDictionary)
                {
                    volumeSum += voxel.Value.GetLeafVolume();
                }
        
                Debug.Log($"Current compactedness is " +
                          $"{volumeSum/ (baseVoxelDictionary.Count*baseVoxelMap.voxelSize*baseVoxelMap.voxelSize*baseVoxelMap.voxelSize)}");
            }
        }

        
        
        public void ShowDebugGizmos()
        {
            if(!IChunkDispatcher.isDebug)
                return;
            //Gizmos.DrawSphere(playerPos,maxViewDistance*Chunk.GetWorldSize()[0]);
            foreach (var voxel in baseVoxelDictionary)
            {
                voxel.Value.DrawLeafGizmos();
                 if(voxel.Value.CalculateChunkCoords()!=null)
                     foreach (var kvPair in voxel.Value.CalculateChunkCoords())
                     {
                         Chunk.DrawChunkGizmo(kvPair.Key);
                         // for(int i = 0; i < kvPair.Value.voxelPositions.Count; i++)
                         // {
                         //     Gizmos.color = Color.green;
                         //     Gizmos.DrawCube(kvPair.Value.voxelPositions[i],Vector3.one*kvPair.Value.voxelSize[i]);
                         // }
                     }
                voxel.Value.DrawGizmo();
            }
        }
    }
}
