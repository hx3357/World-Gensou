using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This module dispatches existing chunks based on certain logic. It's mainly responsible for the life cycle of each
/// chunk by invoking the interface IChunkGenerator.
/// </summary>
public interface IChunkDispatcher
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="chunkGroupSurroundBox"></param>
    /// <param name="activeChunks"></param>
    /// <param name="playerPosition"></param>
    /// <param name="maxViewDistance"></param>
    /// <param name="chunksToGenerate">Value 1: chunk coord, Value 2: chunk resolution</param>
    /// <param name="chunksToDestroy"></param>
    /// <param name="chunkParameters"> The chunk parameter defined by the dispatcher </param>
    public void DispatchChunks(SurroundBox chunkGroupSurroundBox, Dictionary<Vector3Int, int> activeChunks,
        Vector3 playerPosition, float maxViewDistance,
        out List<(Vector3Int coord, int resolution)> chunksToGenerate, out List<Vector3Int> chunksToDestroy,
        out object[] chunkParameters);

    /// <summary>
    /// Implement this to show the debug gizmos such as DrawCube, DrawSphere and so on.
    /// </summary>
    public void ShowDebugGizmos();
}