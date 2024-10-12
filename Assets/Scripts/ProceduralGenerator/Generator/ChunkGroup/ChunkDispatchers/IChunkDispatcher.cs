using System.Collections.Generic;
using UnityEngine;

public interface IChunkDispatcher
{
    public void DispatchChunks(SurroundBox chunkGroupSurroundBox,HashSet<Vector3Int> activeChunks,Vector3 playerPosition, float maxViewDistance,
        out List<Vector3Int> chunksToGenerate,out List<Vector3Int> chunksToDestroy, out List<object> chunkParameters);
    
    public void ShowDebugGizmos();
}