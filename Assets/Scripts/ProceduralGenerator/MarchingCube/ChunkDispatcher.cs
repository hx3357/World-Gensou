using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkDispatcher : MonoBehaviour
{
   public Transform playerTransform;
   
   public float maxViewDistance = 5;
   
   [Header("Marching Cube")]
   [Range(0,1)]
   public float isoSurface = 0.5f;
   public float lerpParam = 0;
   [Header("Scaler Field Generator")]
   public int ocatves = 4;
   public float scale = 40;
   [Range(0,1)]
   public float persistance = 0.5f;
   public float lacunarity = 2;
   public int seed=114514;
   public int maxHeight = 40;
   public AnimationCurve heightMapping;
   [Range(0,1)]
   public float heightOffset=0.446f;
   [Range(0,1)]
   public float heightScale=0.261f;
   [Header("Compute Shader")]
   public ComputeShader marchingCubeCS;
   public ComputeShader downSampleCS;
   
   private Vector3Int playerChunkCoord,lastPlayerChunkCoord;
   
   private IChunkFactory chunkFactory;
   private IScalerFieldGenerator scalerFieldGenerator;
   private IScalerFieldDownSampler downSampler;
   
   private Dictionary<Vector3Int,Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
   
   
   
   private void Start()
   {
      Initialize();
   }

   private void Update()
   {
      Vector3 playerPosition = playerTransform.position;
      playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
      if(playerChunkCoord != lastPlayerChunkCoord)
      {
         UpdateChunks();
         lastPlayerChunkCoord = playerChunkCoord;
      }
   }

   void Initialize()
   {
      Chunk.SetUniversalChunkSize(32 * Vector3Int.one,Vector3.one);
      downSampler = new GPUTrilinearScalerFieldDownSampler(downSampleCS);
      scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator_2D
         (ocatves, scale, persistance, lacunarity, seed,maxHeight,heightMapping,heightOffset,heightScale);
      chunkFactory = gameObject.AddComponent<McChunkFactoryV2>();
      chunkFactory.SetParameters(scalerFieldGenerator,2,downSampler);
      if(chunkFactory is McChunkFactory value)
         value.SetExclusiveParameters(marchingCubeCS,isoSurface,lerpParam);
   }
   
   void UpdateChunks()
   {
      Vector3 playerPosition = playerTransform.position;

      Vector3Int _playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
      
      int celledMaxViewDistance = Mathf.CeilToInt(maxViewDistance)+1;
      
      for(int x = -celledMaxViewDistance;x<=celledMaxViewDistance;x++)
         for(int y = -celledMaxViewDistance;y<=celledMaxViewDistance;y++)
            for(int z = -celledMaxViewDistance;z<=celledMaxViewDistance;z++)
            {
               Vector3Int chunkCoord = _playerChunkCoord + new Vector3Int(x,y,z);
               float distance = Vector3Int.Distance(chunkCoord,_playerChunkCoord);
                 if(distance <= maxViewDistance)
                  {
                     if(!chunks.ContainsKey(chunkCoord))
                     {
                        Chunk chunk = chunkFactory.ProduceChunk(chunkCoord);
                        chunks.Add(chunkCoord,chunk);
                     }
                  }
                  else
                  {
                     if(chunks.ContainsKey(chunkCoord))
                     {
                        Chunk chunk = chunks[chunkCoord];
                        chunk.DestroyChunk();
                        chunks.Remove(chunkCoord);
                     }
                  }
            }
   }
}

