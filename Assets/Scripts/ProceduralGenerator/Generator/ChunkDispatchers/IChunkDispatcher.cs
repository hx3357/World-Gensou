using System;
using System.Collections.Generic;
using UnityEngine;

public interface IChunkDispatcher
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="chunkGroupSurroundBox"></param>
    /// <param name="activeChunks"></param>
    /// <param name="playerPosition"></param>
    /// <param name="maxViewDistance"></param>
    /// <param name="chunksToGenerate"></param>
    /// <param name="chunksToDestroy"></param>
    /// <param name="chunkParameters"> The chunk parameter defined by the dispatcher </param>
    public void DispatchChunks(SurroundBox chunkGroupSurroundBox,HashSet<Vector3Int> activeChunks,
        Vector3 playerPosition, float maxViewDistance, 
        out List<Vector3Int> chunksToGenerate,out List<Vector3Int> chunksToDestroy, out object[] chunkParameters);
    
    public void ShowDebugGizmos();
    
    public static bool isDebug = true;
}