using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SDFIslandGroup : ChunkGroup
{
    private int islandMaxRadius;
    private float islandEmptiness;
    private HashSet<Vector3Int> islandCenters = new HashSet<Vector3Int>();
    
    public override void Initialize(IChunkFactory m_chunkFactory,
        int m_maxViewDistance, Material m_chunkMaterial, int[] m_surroundBox, int m_seed, params object[] parameters)
    {
        islandMaxRadius = (int)parameters[0];
        islandEmptiness = (float)parameters[1];
        base.Initialize(m_chunkFactory, m_maxViewDistance, m_chunkMaterial, m_surroundBox,
            m_seed, parameters);
    }
    protected override void UpdateChunks(Vector3 playerPosition, float m_maxViewDistance)
    {
        //Maintain the island center chunk coords first using the same algorithm as the base class
        Vector3Int _playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
        int celledMaxViewDistance = Mathf.CeilToInt(m_maxViewDistance);
        
        List<Vector3Int> newIslandCenters = new List<Vector3Int>();
        List<Vector3Int> removeIslandCenters = new List<Vector3Int>();
      
        for(int x = -celledMaxViewDistance;x<= celledMaxViewDistance;x++)
        for(int y = -celledMaxViewDistance;y<= celledMaxViewDistance;y++)
        for(int z = -celledMaxViewDistance;z<=celledMaxViewDistance;z++)
        {
            Vector3Int chunkCoord = _playerChunkCoord + new Vector3Int(x,y,z);
            float distance = Vector3Int.Distance(chunkCoord,_playerChunkCoord);
            if(distance <= m_maxViewDistance&&
               ProcedualGeneratorUtility.isInSurroundBox(chunkCoord,surroundBox))
            {
                if(perlinNoise3D.Get3DPerlin(Chunk.GetChunkCenterByCoord(chunkCoord) * (0.01f*islandEmptiness))>1-(1/islandEmptiness))
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
        
        Debug.Log($"new Island Centers: {newIslandCenters.Count}");
        
        //Update the scalar field generator parameters
        scalerFieldParameters = new object[islandCenters.Count];
        int i = 0;
        foreach (var chunkOrigin in islandCenters.Select(Chunk.GetChunkCenterByCoord))
        {
            scalerFieldParameters[i++] = new Vector4(chunkOrigin.x,chunkOrigin.y,chunkOrigin.z,
                0);
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
                Debug.Log($"Produce Chunk");
                chunkFactory.ProduceChunk(islandChunkCoord,chunkMaterial,m_isForceUpdate:true);
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
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var islandCenter in islandCenters)
        {
            Gizmos.DrawSphere(Chunk.GetChunkCenterByCoord(islandCenter),10);
        }
    }
}
