using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A templete used for debug purpose
/// </summary>
public class DebugDispatcher : IChunkDispatcher
{
    public void DispatchChunks(SurroundBox chunkGroupSurroundBox, Dictionary<Vector3Int, int> activeChunks,
        Vector3 playerPosition,
        float maxViewDistance, out List<(Vector3Int coord, int resolution)> chunksToGenerate,
        out List<Vector3Int> chunksToDestroy, out object[] chunkParameters)
    {
        chunksToGenerate = null;
        chunksToDestroy = null;
        chunkParameters = new object[] { };
    }

    public void ShowDebugGizmos()
    {
    }
}