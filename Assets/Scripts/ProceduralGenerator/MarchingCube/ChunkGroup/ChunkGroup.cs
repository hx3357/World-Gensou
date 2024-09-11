using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGroup : MonoBehaviour
{
   public int maxViewDistance;
   public Material chunkMaterial;

   private int seed;
   
   private IScalerFieldGenerator scalerFieldGenerator;
   protected object[] scalerFieldParameters;
   
   protected IChunkFactory chunkFactory;
   protected HashSet<Vector3Int> activeChunks = new HashSet<Vector3Int>();
   protected int[] surroundBox;

   protected PerlinNoise3D perlinNoise3D;
   
   private bool isFirstUpdating;
   private bool isFirstUpdated;
   private int firstUpdateFrameInterval = 10;
   
   /// <summary>
   /// 
   /// </summary>
   /// <param name="m_chunkFactory"></param>
   /// <param name="m_maxViewDistance"></param>
   /// <param name="m_chunkMaterial"></param>
   /// <param name="m_surroundBox"></param>
   /// <param name="m_seed"></param>
   /// <param name="parameters">Parameters for scalar field generator</param>
   public virtual void Initialize(IChunkFactory m_chunkFactory,
      int m_maxViewDistance,Material m_chunkMaterial,int[] m_surroundBox,int m_seed,params object[] parameters)
   {
      scalerFieldGenerator = m_chunkFactory.GetScalerFieldGenerator();
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
   
   protected virtual void UpdateChunks(Vector3 playerPosition,float m_maxViewDistance)
   {
      Vector3Int _playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
      int celledMaxViewDistance = Mathf.CeilToInt(m_maxViewDistance)+1;
      List<Vector3Int> preproducedChunks = new List<Vector3Int>();
      
      for(int x = -celledMaxViewDistance;x<= celledMaxViewDistance;x++)
         for(int y = -celledMaxViewDistance;y<= celledMaxViewDistance;y++)
            for(int z = -celledMaxViewDistance;z<=celledMaxViewDistance;z++)
            {
               Vector3Int chunkCoord = _playerChunkCoord + new Vector3Int(x,y,z);
               float distance = Vector3Int.Distance(chunkCoord,_playerChunkCoord);
                 if(distance <= m_maxViewDistance&&ProcedualGeneratorUtility.isInSurroundBox(chunkCoord,surroundBox))
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
   
   private void FirstUpdate(Vector3 playerPosition)
   {
      isFirstUpdating = true;
      StartCoroutine(FirstUpdateCoroutine(playerPosition));
   }
   
   IEnumerator FirstUpdateCoroutine(Vector3 playerPosition)
   {
      
      for(int viewDistance = 1;viewDistance<=maxViewDistance;viewDistance++)
      {
         UpdateChunks(playerPosition,viewDistance);
         for(int frame = 0;frame<firstUpdateFrameInterval;frame++)
            yield return null;
      }
      
      isFirstUpdating = false;
      isFirstUpdated = true;
     
   }

   public void UpdateChunkGroup(Vector3 playerPosition)
   {
      if(isFirstUpdating)return;
      if (!isFirstUpdated)
      {
         FirstUpdate(playerPosition);
         return;
      }
      UpdateChunks( playerPosition,maxViewDistance);
   }
}
