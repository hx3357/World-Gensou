using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGroup : MonoBehaviour
{
   public float maxViewDistance;
   public Material chunkMaterial;

   private int seed;
   
   private IScalerFieldGenerator scalerFieldGenerator;
   protected object[] scalerFieldParameters;
   
   protected IChunkFactory chunkFactory;
   protected HashSet<Vector3Int> activeChunks = new HashSet<Vector3Int>();
   protected int[] surroundBox;

   protected PerlinNoise3D perlinNoise3D;
   
   public virtual void Initialize(IScalerFieldGenerator m_scalerFieldGenerator,IChunkFactory m_chunkFactory,
      float m_maxViewDistance,Material m_chunkMaterial,int[] m_surroundBox,int m_seed,params object[] parameters)
   {
      scalerFieldGenerator = m_scalerFieldGenerator;
      chunkFactory = m_chunkFactory;
      surroundBox = m_surroundBox is { Length: 6 } ? m_surroundBox : 
         new []{int.MaxValue,int.MinValue,int.MaxValue,int.MinValue,int.MaxValue,int.MinValue};
      maxViewDistance = m_maxViewDistance;
      chunkMaterial = m_chunkMaterial;
      scalerFieldParameters = parameters;
      seed = m_seed;
      perlinNoise3D = new PerlinNoise3D();
      perlinNoise3D.SetRandomSeed(seed);
   }
   
   /// <summary>
   /// Used to calculate parameters for the scalar field generator
   /// </summary>
   protected void PrepareScalarFieldGeneratorParameters()
   {
      if(scalerFieldParameters != null)
         scalerFieldGenerator.SetParameters(scalerFieldParameters);
   }
   
   public virtual void UpdateChunks(Vector3 playerPosition)
   {
      Vector3Int _playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
      int celledMaxViewDistance = Mathf.CeilToInt(maxViewDistance)+1;
      List<Vector3Int> preproducedChunks = new List<Vector3Int>();
      
      for(int x = -celledMaxViewDistance;x<= celledMaxViewDistance;x++)
         for(int y = -celledMaxViewDistance;y<= celledMaxViewDistance;y++)
            for(int z = -celledMaxViewDistance;z<=celledMaxViewDistance;z++)
            {
               Vector3Int chunkCoord = _playerChunkCoord + new Vector3Int(x,y,z);
               float distance = Vector3Int.Distance(chunkCoord,_playerChunkCoord);
                 if(distance <= maxViewDistance&&ProcedualGeneratorUtility.isInSurroundBox(chunkCoord,surroundBox))
                  {
                     if(!activeChunks.Contains(chunkCoord))
                     {
                        preproducedChunks.Add(chunkCoord);
                     }
                  }
                  else
                  {
                     if(activeChunks.Contains(chunkCoord))
                     {
                        chunkFactory.DeleteChunk(chunkCoord);
                        activeChunks.Remove(chunkCoord);
                     }
                  }
            }
      
      PrepareScalarFieldGeneratorParameters();

      foreach (var chunkCoord in preproducedChunks)
      {
         chunkFactory.ProduceChunk(chunkCoord,chunkMaterial);
         activeChunks.Add(chunkCoord);
      }
   }
}
