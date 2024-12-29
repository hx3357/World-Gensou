using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Profiling;
using Object = UnityEngine.Object;

namespace ChunkDispatchers.VoxelBasedDispatch
{
    /// <summary>
    /// <para>Suitable for sdf objects which size varies greatly.</para>
    /// <para>Base voxel is generated around the player when player is approaching.</para>
    /// <para>The dot count parameter of each base voxel is calculated based on possion distribution</para>
    /// <para>When the dot count is greater than 1, the voxel will be spilted into 8 sub voxels just like an octree and
    /// distribute its dot count to the sub voxels.</para>
    /// <para>When the dot count is 1, the chunk will be generated inside the voxel.</para>
    /// <para>When the dot count is 0, the voxel will be ignored.</para>
    /// <para>For the sake of extra randomness, an back track algorithm is used to randomly displace the sub voxels</para>
    /// <para>The module will do this process recursively.</para>
    /// <para>The module is also responsible for LOD management and chunk destroying.</para>
    /// </summary>
    // TODO: Decopling LOD management and chunk destroying by implementing composite pattern
    public class VoxelBasedRandomPointDispatcher : IChunkDispatcher
    {
        private readonly VoxelMap baseVoxelMap;

        // readonly VoxelMap[] voxelMaps;
        private readonly float dotCountExpection;

        // Key: voxel coord
        private Dictionary<Vector3Int, Voxel> baseVoxelDictionary = new();
        private Vector3Int lastPlayerVoxelCoord;
        private bool isFirstTime = true;
        private readonly Dictionary<Vector3Int, ChunkParameter> chunkCoordMap = new();
        private Action<Voxel> onVoxelGenerated;
        private ChunkLODManager lodManager;

        public VoxelBasedRandomPointDispatcher(float dotCountExpection = 1f, int voxelChunkSize = 7,
            Action<Voxel> m_onVoxelGenerated = null, ChunkLODManager m_lodManager = null)
        {
            lodManager = Object.FindObjectOfType<ChunkLODManager>();
            this.dotCountExpection = dotCountExpection;
            baseVoxelMap = new VoxelMap(voxelChunkSize);
            onVoxelGenerated = m_onVoxelGenerated;
            if (m_lodManager != null) lodManager = m_lodManager;
        }

        private class GenerateChunkData
        {
            public readonly List<(Vector3Int, int)> chunksToGenerate = new();
            public readonly List<object> chunkParametersList = new();

            public void AddChunk(Vector3Int chunkCoord, int chunkResolution, object chunkParameter)
            {
                chunksToGenerate.Add((chunkCoord, chunkResolution));
                chunkParametersList.Add(chunkParameter);
            }
        }

        public void DispatchChunks(SurroundBox chunkGroupSurroundBox, Dictionary<Vector3Int, int> activeChunks,
            Vector3 playerPosition, float maxViewDistance,
            out List<(Vector3Int coord, int resolution)> chunksToGenerate, out List<Vector3Int> chunksToDestroy,
            out object[] chunkParameters)
        {
            var _playerVoxelCoord = baseVoxelMap.GetVoxelCoordByPosition(playerPosition);
            var celledMaxViewedVoxelRadius =
                Mathf.CeilToInt(maxViewDistance * Chunk.GetWorldSize()[0] / baseVoxelMap.voxelSize);
            var flooredMaxViewedVoxelRadius =
                Mathf.FloorToInt(maxViewDistance * Chunk.GetWorldSize()[0] / baseVoxelMap.voxelSize);
            var updateVoxelRadius = celledMaxViewedVoxelRadius + 1;

            // Generate new base voxel when the player enters new voxel coord.
            if (isFirstTime || _playerVoxelCoord != lastPlayerVoxelCoord)
            {
                // Generate new base voxel which recursively spilts into sub voxels
                for (var x = -updateVoxelRadius; x <= updateVoxelRadius; x++)
                for (var y = -updateVoxelRadius; y <= updateVoxelRadius; y++)
                for (var z = -updateVoxelRadius; z <= updateVoxelRadius; z++)
                {
                    var voxelCoord = _playerVoxelCoord + new Vector3Int(x, y, z);
                    var voxelOrigin = baseVoxelMap.GetVoxelOriginByCoord(voxelCoord);
                    var distance = Vector3Int.Distance(voxelCoord, _playerVoxelCoord);
                    var curChunkResolution =
                        lodManager.GetChunkResolutionByDistance(Vector3.Distance(playerPosition, voxelOrigin));
                    if (distance < celledMaxViewedVoxelRadius)
                    {
                        var newVoxel = new Voxel(voxelOrigin, baseVoxelMap.voxelSize, 0,
                            true, null, initDotExp: dotCountExpection, onGenerate: onVoxelGenerated,
                            _subVoxelExpandFactor: 0.5f)
                        {
                            chunkResolution = curChunkResolution
                        };
                        newVoxel.CalculateChunkCoords(chunkCoordMap);
                        baseVoxelDictionary.TryAdd(voxelCoord, newVoxel);
                    }
                }

                foreach (var voxelCoord in baseVoxelDictionary.Keys.ToArray())
                {
                    var distance = Vector3Int.Distance(voxelCoord, _playerVoxelCoord);
                    if (distance > celledMaxViewedVoxelRadius)
                    {
                        var destroyVoxel = baseVoxelDictionary[voxelCoord];
                        destroyVoxel.RemoveChunkCoords(chunkCoordMap);
                        baseVoxelDictionary.Remove(voxelCoord);
                    }
                }

                lastPlayerVoxelCoord = _playerVoxelCoord;
            }

            GenerateChunkData generateChunkData = new();
            List<Vector3Int> _chunksToDestroy = new();


            Parallel.ForEach(chunkCoordMap.Keys, chunkCoord =>
            {
                var playerChunkDistance = Vector3.Distance(Chunk.GetChunkCenterByCoord(chunkCoord), playerPosition);
                var curViewDistance = maxViewDistance * Chunk.GetWorldSize()[0];
                var curChunkResolution = lodManager.GetChunkResolutionByDistance(playerChunkDistance);

                var isContain = activeChunks.ContainsKey(chunkCoord);
                var isPlayerNearChunk = playerChunkDistance < curViewDistance;

                if (isPlayerNearChunk && (!isContain || activeChunks[chunkCoord] != curChunkResolution))
                    lock (generateChunkData)
                    {
                        generateChunkData.AddChunk(chunkCoord, curChunkResolution, chunkCoordMap[chunkCoord]);
                        return;
                    }

                if (isContain && !isPlayerNearChunk)
                    lock (_chunksToDestroy)
                    {
                        _chunksToDestroy.Add(chunkCoord);
                    }
            });

            chunksToGenerate = generateChunkData.chunksToGenerate;
            chunkParameters = generateChunkData.chunkParametersList.ToArray();
            chunksToDestroy = _chunksToDestroy;


            foreach (var chunk in chunksToDestroy) chunkCoordMap.Remove(chunk);

            isFirstTime = false;

            if (DebugWhiteboard.Instance.isDebugChunkDispatcher)
            {
                float volumeSum = 0;
                foreach (var voxel in baseVoxelDictionary) volumeSum += voxel.Value.GetLeafVolume();

                Debug.Log($"Current compactedness is " +
                          $"{volumeSum / (baseVoxelDictionary.Count * baseVoxelMap.voxelSize * baseVoxelMap.voxelSize * baseVoxelMap.voxelSize)}");
            }
        }


        public void ShowDebugGizmos()
        {
            if (!DebugWhiteboard.Instance.isDebugChunkDispatcher)
                return;
            foreach (var voxel in baseVoxelDictionary)
                //voxel.Value.DrawLeafGizmos();
                // foreach (var coord in voxel.Value.GetChunkCoords())
                // {
                //     Vector3 hash = HashUtility.Get3DHash(voxel.Key);
                //     Gizmos.color = new Color(hash.x, hash.y, hash.z, 1f);
                //     Chunk.DrawChunkGizmo(coord);
                // }
                voxel.Value.DrawGizmo();

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