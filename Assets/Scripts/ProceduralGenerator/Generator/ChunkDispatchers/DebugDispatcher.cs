
using System.Collections.Generic;
using UnityEngine;

public class DebugDispatcher : IChunkDispatcher
{
    int counter = 0;
    
    public void DispatchChunks(in SurroundBox chunkGroupSurroundBox, in Dictionary<Vector3Int,int> activeChunks, in Vector3 playerPosition,
        in float maxViewDistance, ref List<(Vector3Int, int)> chunksToGenerate, ref List<Vector3Int> chunksToDestroy, ref object[] chunkParameters)
    {
        chunksToGenerate = new();
        chunksToGenerate.Add((Vector3Int.zero, counter++%2 == 0? 3:31));
        chunkParameters = new object[] { new SDFIslandSFGParameter( new []{ new Vector4(0, 0, 0, 0) },
            new [] { new Vector4(10, 10, 0, 0) }) };
    }

    public void ShowDebugGizmos()
    {
        
    }
}
