using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SDFIslandGroup : ChunkGroup
{
    private int islandMaxRadius;
    private HashSet<Vector3Int> islandCenters = new HashSet<Vector3Int>();
    
    public override void Initialize(IScalerFieldGenerator m_scalerFieldGenerator, IChunkFactory m_chunkFactory,
        float m_maxViewDistance, Material m_chunkMaterial, int[] m_surroundBox, int m_seed, params object[] parameters)
    {
        islandMaxRadius = (int)parameters[0];
        base.Initialize(m_scalerFieldGenerator, m_chunkFactory, m_maxViewDistance, m_chunkMaterial, m_surroundBox,
            m_seed, parameters);
    }
    
    // protected override void PrepareScalarFieldGeneratorParameters(List<Vector3Int> preproducedChunkCoords)
    // {
    //     List<object> parameters = new List<object>();
    //     foreach (var chunkCoord in preproducedChunkCoords)
    //     {
    //         Vector3 chunkOrigin = Chunk.GetChunkCenterByCoord(chunkCoord);
    //         if (perlinNoise3D.Get3DPerlin(chunkOrigin*0.51f)>0.9f)
    //         {
    //             parameters.Add(chunkOrigin);
    //         }
    //     }
    //     scalerFieldParameters = parameters.ToArray();
    //     base.PrepareScalarFieldGeneratorParameters(preproducedChunkCoords);
    // }

    public override void UpdateChunks(Vector3 playerPosition)
    {
        //Maintain the island center chunk coords first using the same algorithm as the base class
        Vector3Int _playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
        int celledMaxViewDistance = Mathf.CeilToInt(maxViewDistance);
        
        List<Vector3Int> newIslandCenters = new List<Vector3Int>();
        List<Vector3Int> removeIslandCenters = new List<Vector3Int>();
      
        for(int x = -celledMaxViewDistance;x<= celledMaxViewDistance;x++)
        for(int y = -celledMaxViewDistance;y<= celledMaxViewDistance;y++)
        for(int z = -celledMaxViewDistance;z<=celledMaxViewDistance;z++)
        {
            Vector3Int chunkCoord = _playerChunkCoord + new Vector3Int(x,y,z);
            float distance = Vector3Int.Distance(chunkCoord,_playerChunkCoord);
            if(distance <= maxViewDistance&&
               ProcedualGeneratorUtility.isInSurroundBox(chunkCoord,surroundBox))
            {
                if(perlinNoise3D.Get3DPerlin(Chunk.GetChunkCenterByCoord(chunkCoord)*0.1f)>0.95f)
                {
                    islandCenters.Add(chunkCoord);
                    newIslandCenters.Add(chunkCoord);
                }
            }
            else
            {
                if(islandCenters.Contains(chunkCoord))
                {
                    islandCenters.Remove(chunkCoord);
                    removeIslandCenters.Add(chunkCoord);
                }
            }
        }
        
        //Update the scalar field generator parameters
        scalerFieldParameters = new object[newIslandCenters.Count];
        for(int i = 0;i<newIslandCenters.Count;i++)
        {
            Vector3 chunkOrigin = Chunk.GetChunkCenterByCoord(newIslandCenters[i]);
            scalerFieldParameters[i] = chunkOrigin;
        }
        PrepareScalarFieldGeneratorParameters();
        
        //Update the chunks of the island
        //Produce the new chunks
        foreach (var chunkCoord in newIslandCenters)
        {
            for(int x = -islandMaxRadius;x<= islandMaxRadius;x++)
            for(int y = -islandMaxRadius;y<= islandMaxRadius;y++)
            for (int z = -islandMaxRadius; z <= islandMaxRadius; z++)
            {
                Vector3Int islandChunkCoord = chunkCoord + new Vector3Int(x, y, z);
                if(activeChunks.Contains(islandChunkCoord)) continue;
                chunkFactory.ProduceChunk(islandChunkCoord,chunkMaterial);
                activeChunks.Add(islandChunkCoord);
            }
        }
        
        //Remove the chunks that are too far away from the player
        foreach (var chunkCoord in removeIslandCenters)
        {
            for(int x = -islandMaxRadius;x<= islandMaxRadius;x++)
            for(int y = -islandMaxRadius;y<= islandMaxRadius;y++)
            for (int z = -islandMaxRadius; z <= islandMaxRadius; z++)
            {
                Vector3Int islandChunkCoord = chunkCoord + new Vector3Int(x, y, z);
                if(!activeChunks.Contains(islandChunkCoord)) continue;
                chunkFactory.DeleteChunk(islandChunkCoord);
                activeChunks.Remove(islandChunkCoord);
            }
        }
    }
}
