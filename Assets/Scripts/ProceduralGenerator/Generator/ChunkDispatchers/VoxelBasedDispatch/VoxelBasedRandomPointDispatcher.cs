using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace ChunkDispatchers.VoxelBasedDispatch
{
    

    /// <summary>
    /// <para>Designed for sdf island SFG</para>
    /// 
    /// <para>Suitable for surrounding boxs which size varies greatly.</para>
    /// <para>When the dot count is greater than 1, the voxel will be cut into 8 sub voxels and distribute its dot count to the sub voxels.</para>
    /// <para>When the dot count is 1, the chunk will be generated inside the voxel.</para>
    /// <para>When the dot count is 0, the voxel will be ignored.</para>
    /// <para>Surrounding boxs whose size is close to each other will be more likely to stay together.</para>
    /// </summary>
    public class VoxelBasedRandomPointDispatcher : IChunkDispatcher
    {
        readonly VoxelMap baseVoxelMap;

        // readonly VoxelMap[] voxelMaps;
        readonly float dotCountExpection;

        // Key: voxel coord
        private Dictionary<Vector3Int, Voxel> baseVoxelDictionary = new();
        private Vector3Int lastPlayerVoxelCoord;
        private bool isFirstTime = true;
        private Dictionary<Vector3Int,ChunkParameter> chunkCoordMap = new();
        private Action<Voxel> onVoxelGenerated;

        public VoxelBasedRandomPointDispatcher(float dotCountExpection = 1f,int voxelChunkSize = 8,
            Action<Voxel> m_onVoxelGenerated = null)
        {
            this.dotCountExpection = dotCountExpection;
            baseVoxelMap = new VoxelMap(voxelChunkSize);
            onVoxelGenerated = m_onVoxelGenerated;
        }


        public void DispatchChunks(SurroundBox chunkGroupSurroundBox, HashSet<Vector3Int> activeChunks,
            Vector3 playerPosition, float maxViewDistance,
            out List<Vector3Int> chunksToGenerate, out List<Vector3Int> chunksToDestroy,
            out object[] chunkParameters)
        {
            chunksToGenerate = new();
            chunksToDestroy = new();
            List<object> chunkParametersList = new();
            
            List<Voxel> generatedNewVoxels = new();

            Vector3Int _playerVoxelCoord = baseVoxelMap.GetVoxelCoordByPosition(playerPosition);
            int celledMaxViewedVoxelRadius =
                Mathf.CeilToInt(maxViewDistance * Chunk.GetWorldSize()[0] / baseVoxelMap.voxelSize) + 1;
            
            if (isFirstTime || _playerVoxelCoord != lastPlayerVoxelCoord)
            {
                for (int x = -celledMaxViewedVoxelRadius; x <= celledMaxViewedVoxelRadius; x++)
                for (int y = -celledMaxViewedVoxelRadius; y <= celledMaxViewedVoxelRadius; y++)
                for (int z = -celledMaxViewedVoxelRadius; z <= celledMaxViewedVoxelRadius; z++)
                {
                    Vector3Int voxelCoord = _playerVoxelCoord + new Vector3Int(x, y, z);
                    Vector3 voxelOrigin = baseVoxelMap.GetVoxelOriginByCoord(voxelCoord);
                    float distance = Vector3Int.Distance(voxelCoord, _playerVoxelCoord);
                    if (distance < celledMaxViewedVoxelRadius)
                    {
                        Voxel newVoxel = new Voxel(voxelOrigin, baseVoxelMap.voxelSize, 0,
                            true, initDotExp: dotCountExpection,onGenerate: onVoxelGenerated,_subVoxelExpandFactor: 0.5f);
                        baseVoxelDictionary.TryAdd(voxelCoord, newVoxel);
                        generatedNewVoxels.Add(newVoxel);
                    }
                }

                lastPlayerVoxelCoord = _playerVoxelCoord;

                foreach (var voxelCoord in baseVoxelDictionary.Keys.ToArray())
                {
                    float distance = Vector3Int.Distance(voxelCoord, _playerVoxelCoord);
                    if (distance > celledMaxViewedVoxelRadius)
                    {
                        baseVoxelDictionary.Remove(voxelCoord);
                    }
                }
            }
            
            foreach (var voxel in generatedNewVoxels)
            {
                voxel.CalculateChunkCoords(chunkCoordMap);
            }
            
            foreach (var chunkCoord in chunkCoordMap)
            {
                float playerChunkDistance = Vector3.Distance(Chunk.GetChunkCenterByCoord(chunkCoord.Key), playerPosition);
                float curViewDistance = EllipticalDistance.GetDistance(playerPosition - Chunk.GetChunkCenterByCoord(chunkCoord.Key),
                    maxViewDistance * Chunk.GetWorldSize()[0], maxViewDistance * 0.5f * Chunk.GetWorldSize()[0]);
                
                if (!activeChunks.Contains(chunkCoord.Key) &&
                    playerChunkDistance < curViewDistance)
                {
                    chunksToGenerate.Add(chunkCoord.Key);
                    chunkParametersList.Add(chunkCoord.Value);
                }

                if (activeChunks.Contains(chunkCoord.Key) &&
                    playerChunkDistance > curViewDistance)
                {
                    chunksToDestroy.Add(chunkCoord.Key);
                }
            }

            isFirstTime = false;
            chunkParameters = chunkParametersList.ToArray();

            if (DebugWhiteboard.Instance.isDebugChunkDispatcher)
            {
                float volumeSum = 0;
                foreach (var voxel in baseVoxelDictionary)
                {
                    volumeSum += voxel.Value.GetLeafVolume();
                }

                Debug.Log($"Current compactedness is " +
                          $"{volumeSum / (baseVoxelDictionary.Count * baseVoxelMap.voxelSize * baseVoxelMap.voxelSize * baseVoxelMap.voxelSize)}");
            }
        }


        public void ShowDebugGizmos()
        {
            if (!DebugWhiteboard.Instance.isDebugChunkDispatcher)
                return;
            foreach (var voxel in baseVoxelDictionary)
            {
                voxel.Value.DrawLeafGizmos();
                    
                // foreach (var coord in voxel.Value.GetChunkCoords())
                // {
                //     Vector3 hash = HashUtility.Get3DHash(voxel.Key);
                //     Gizmos.color = new Color(hash.x, hash.y, hash.z, 1f);
                //     Chunk.DrawChunkGizmo(coord);
                // }
                voxel.Value.DrawGizmo();
            }
            
            // foreach (var kvPair in chunkCoordMap)
            // {
            //     Vector3 hash = HashUtility.Get3DHash(voxel.Key);
            //     Gizmos.color = new Color(hash.x, hash.y, hash.z, 1f);
            //     Chunk.DrawChunkGizmo(kvPair.Key);
            //     // for(int i = 0; i < kvPair.Value.voxelPositions.Count; i++)
            //     // {
            //     //     Gizmos.color = Color.green;
            //     //     Gizmos.DrawCube(kvPair.Value.voxelPositions[i],Vector3.one*kvPair.Value.voxelSize[i]);
            //     // }
            // }
        }
    }
}