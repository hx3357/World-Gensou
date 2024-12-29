using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>Generate chunks in a spherical area around the player based on the max view distance.</para>
/// <para>Directly generate chunks in the spherical area and destroy chunks outside the spherical area.</para>
/// <para>No chunk parameters are generated. </para>
/// </summary>
public class SphericalDispatcher : IChunkDispatcher
{
    private readonly int chunkResolution;

    public SphericalDispatcher(int m_chunkResolution)
    {
        chunkResolution = m_chunkResolution;
    }

    public void DispatchChunks(SurroundBox chunkGroupSurroundBox, Dictionary<Vector3Int, int> activeChunks,
        Vector3 playerPosition, float maxViewDistance,
        out List<(Vector3Int coord, int resolution)> chunksToGenerate, out List<Vector3Int> chunksToDestroy,
        out object[] chunkParameters)
    {
        var _playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
        var celledMaxViewDistance = Mathf.CeilToInt(maxViewDistance) + 1;
        chunksToGenerate = new List<(Vector3Int, int)>();
        chunksToDestroy = new List<Vector3Int>();

        for (var x = -celledMaxViewDistance; x <= celledMaxViewDistance; x++)
        for (var y = -celledMaxViewDistance; y <= celledMaxViewDistance; y++)
        for (var z = -celledMaxViewDistance; z <= celledMaxViewDistance; z++)
        {
            var chunkCoord = _playerChunkCoord + new Vector3Int(x, y, z);
            var distance = Vector3Int.Distance(chunkCoord, _playerChunkCoord);
            if (distance <= maxViewDistance && chunkGroupSurroundBox.IsInSurroundBox(chunkCoord))
            {
                if (!activeChunks.ContainsKey(chunkCoord)) chunksToGenerate.Add((chunkCoord, chunkResolution));
            }
            else
            {
                if (activeChunks.ContainsKey(chunkCoord)) chunksToDestroy.Add(chunkCoord);
            }
        }

        chunkParameters = null;
    }

    public void ShowDebugGizmos()
    {
    }
}