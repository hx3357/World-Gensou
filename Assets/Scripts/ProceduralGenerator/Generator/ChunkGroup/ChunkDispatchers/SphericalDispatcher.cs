using System;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class SphericalDispatcher : IChunkDispatcher
{
    public void DispatchChunks(SurroundBox chunkGroupSurroundBox,HashSet<Vector3Int> activeChunks, Vector3 playerPosition, float maxViewDistance, 
        out List<Vector3Int> chunksToGenerate, out List<Vector3Int> chunksToDestroy, out List<object> chunkParameters)
    {
        Vector3Int _playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
        int celledMaxViewDistance = Mathf.CeilToInt(maxViewDistance)+1;
        chunksToGenerate = new List<Vector3Int>();
        chunksToDestroy = new List<Vector3Int>();
        chunkParameters = null;
      
        for(int x = -celledMaxViewDistance;x<= celledMaxViewDistance;x++)
        for(int y = -celledMaxViewDistance;y<= celledMaxViewDistance;y++)
        for(int z = -celledMaxViewDistance;z<=celledMaxViewDistance;z++)
        {
            Vector3Int chunkCoord = _playerChunkCoord + new Vector3Int(x,y,z);
            float distance = Vector3Int.Distance(chunkCoord,_playerChunkCoord);
            if(distance <= maxViewDistance&&chunkGroupSurroundBox.IsInSurroundBox(chunkCoord))
            {
                if(!activeChunks.Contains(chunkCoord))
                {
                    chunksToGenerate.Add(chunkCoord);
                }
            }
            else
            {
                if(activeChunks.Contains(chunkCoord))
                {
                    chunksToDestroy.Add(chunkCoord);
                }
            }
        }
    }
}
