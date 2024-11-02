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
    
    int chunkResolution = 31;
    
    public SphericalDispatcher(int m_chunkResolution)
    {
        chunkResolution = m_chunkResolution;
    }
    
    public void DispatchChunks( SurroundBox chunkGroupSurroundBox, Dictionary<Vector3Int,int> activeChunks,
         Vector3 playerPosition, float maxViewDistance, 
        ref List<(Vector3Int,int)> chunksToGenerate, ref List<Vector3Int> chunksToDestroy, ref object[] chunkParameters)
    {
        Vector3Int _playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
        int celledMaxViewDistance = Mathf.CeilToInt(maxViewDistance)+1;
        chunksToGenerate = new List<(Vector3Int,int)>();
        chunksToDestroy = new List<Vector3Int>();
      
        for(int x = -celledMaxViewDistance;x<= celledMaxViewDistance;x++)
        for(int y = -celledMaxViewDistance;y<= celledMaxViewDistance;y++)
        for(int z = -celledMaxViewDistance;z<=celledMaxViewDistance;z++)
        {
            Vector3Int chunkCoord = _playerChunkCoord + new Vector3Int(x,y,z);
            float distance = Vector3Int.Distance(chunkCoord,_playerChunkCoord);
            if(distance <= maxViewDistance&&chunkGroupSurroundBox.IsInSurroundBox(chunkCoord))
            {
                if(!activeChunks.ContainsKey(chunkCoord))
                {
                    chunksToGenerate.Add((chunkCoord,chunkResolution));
                }
            }
            else
            {
                if(activeChunks.ContainsKey(chunkCoord))
                {
                    chunksToDestroy.Add(chunkCoord);
                }
            }
        }
    }
    
    public void ShowDebugGizmos() { }
}
