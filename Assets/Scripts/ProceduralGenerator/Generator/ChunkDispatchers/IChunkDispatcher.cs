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
    /// <param name="chunksToGenerate">Value 1: chunk coord, Value 2: chunk resolution</param>
    /// <param name="chunksToDestroy"></param>
    /// <param name="chunkParameters"> The chunk parameter defined by the dispatcher </param>
    public void DispatchChunks(in SurroundBox chunkGroupSurroundBox,in Dictionary<Vector3Int,int> activeChunks,
        in Vector3 playerPosition,in float maxViewDistance, 
       ref List<(Vector3Int,int)> chunksToGenerate,ref List<Vector3Int> chunksToDestroy,ref object[] chunkParameters);
    
    public void ShowDebugGizmos();
    
    public static bool isDebug = true;
}