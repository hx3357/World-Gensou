using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkDispatcher : MonoBehaviour
{
   public Transform playerTransform;
   
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
   
   /// <summary>
   /// Max view distance of each LOD level
   /// </summary>
   public List<int> lodViewDistanceTable;
   
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

   Chunk.LODLevel GetLODLevelByDistance(float distance)
   {
      for(int i = 0;i<lodViewDistanceTable.Count;i++)
      {
         if(distance <= lodViewDistanceTable[i])
            return (Chunk.LODLevel)i;
      }
      return (Chunk.LODLevel)int.MaxValue;
   }

   void Initialize()
   {
      Chunk.SetUniversalChunkSize(32 * Vector3Int.one,Vector3.one);
      downSampler = new GPUTrilinearScalerFieldDownSampler(downSampleCS);
      scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator_2D
         (ocatves, scale, persistance, lacunarity, seed,maxHeight,heightMapping,heightOffset,heightScale);
      chunkFactory = gameObject.AddComponent<McChunkFactoryV2>();
      chunkFactory.SetParameters(scalerFieldGenerator,1,downSampler);
      if(chunkFactory is McChunkFactory value)
         value.SetExclusiveParameters(marchingCubeCS,isoSurface,lerpParam);
   }
   
   void UpdateChunks()
   {
      Vector3 playerPosition = playerTransform.position;

      Vector3Int _playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
      
      int maxViewDistance = lodViewDistanceTable[^1];
      
      for(int x = -maxViewDistance;x<=maxViewDistance;x++)
         for(int y = -maxViewDistance;y<=maxViewDistance;y++)
            for(int z = -maxViewDistance;z<=maxViewDistance;z++)
            {
               Vector3Int chunkCoord = _playerChunkCoord + new Vector3Int(x,y,z);
               float distance = Vector3Int.Distance(chunkCoord,_playerChunkCoord);
               Chunk.LODLevel currentLodLevel = GetLODLevelByDistance(distance);
               if (currentLodLevel == (Chunk.LODLevel)int.MaxValue)
               {
                  if(chunks.ContainsKey(chunkCoord))
                  {
                     chunks[chunkCoord].DestroyChunk();
                     chunks.Remove(chunkCoord);
                  }
                  continue;
               }
               if(!chunks.ContainsKey(chunkCoord))
               {
                  Chunk chunk = chunkFactory.ProduceChunk(chunkCoord,currentLodLevel);
                  chunks.Add(chunkCoord,chunk);
               }
               else
               {
                  Chunk currentChunk = chunks[chunkCoord];
                  if (currentChunk.lodLevel != currentLodLevel)
                     chunkFactory.SetChunk(currentChunk,currentLodLevel);
               }
            }
   }
}

