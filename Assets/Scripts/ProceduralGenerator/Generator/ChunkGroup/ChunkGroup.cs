using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class ChunkGroup : MonoBehaviour
{
   public int maxViewDistance;
   public Material chunkMaterial;

   private int seed;
   
   private IScalerFieldGenerator scalerFieldGenerator;
   protected object[] scalerFieldParameters;
   
   protected IChunkFactory chunkFactory;
   protected HashSet<Vector3Int> activeChunks {get; private set;} = new ();
   protected SurroundBox surroundBox;

   protected PerlinNoise3D perlinNoise3D;
   
   private int chunksNumPerGenerate = 50;
   private int chunksGenerationInterval = 1;
   
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
      int m_maxViewDistance,Material m_chunkMaterial,SurroundBox m_surroundBox,int m_seed,params object[] parameters)
   {
      scalerFieldGenerator = m_chunkFactory.GetScalerFieldGenerator();
      chunkFactory = m_chunkFactory;
      surroundBox = m_surroundBox != null ? m_surroundBox : 
         new SurroundBox(int.MinValue,int.MaxValue,int.MinValue,int.MaxValue,int.MinValue,int.MaxValue);
      maxViewDistance = m_maxViewDistance;
      chunkMaterial = m_chunkMaterial;
      scalerFieldParameters = parameters;
      seed = m_seed;
      perlinNoise3D = new PerlinNoise3D();
      perlinNoise3D.SetRandomSeed(seed);
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
               if(distance <= m_maxViewDistance&&surroundBox.IsInSurroundBox(chunkCoord))
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
      
      StartCoroutine(AsyncLoadChunksCoroutine(preproducedChunks));
   }
   
   /// <summary>
   /// 
   /// </summary>
   /// <param name="chunksToBeProduced"></param>
   /// <param name="m_parameters">A list contains chunk exclusive SFG paramaters if is not null</param>
   /// <returns></returns>
   IEnumerator AsyncLoadChunksCoroutine(List<Vector3Int> chunksToBeProduced, List<object> m_parameters = null)
   {
      if(m_parameters != null)
         Assert.IsTrue(chunksToBeProduced.Count == m_parameters.Count,"Parameters count should be equal to chunks count");
      
      float startTime = Time.realtimeSinceStartup;
      
      for(int i=0;i<chunksToBeProduced.Count;)
      {
         for(int j=0;j<chunksNumPerGenerate&&i<chunksToBeProduced.Count;j++)
         {
            activeChunks.Add(chunksToBeProduced[i]);
            chunkFactory.ProduceChunk(chunksToBeProduced[i++],m_chunkMaterial: chunkMaterial,
               SFGParameters: m_parameters == null ? scalerFieldParameters : m_parameters.ToArray());
         }
         for(int j=0;j<chunksGenerationInterval;j++)
            yield return null;
      }
      
      float duration = Time.realtimeSinceStartup - startTime;
      Debug.Log($"Generate {chunksToBeProduced.Count} chunks in {duration} seconds\n " +
                $"Average: {duration/chunksToBeProduced.Count} seconds per chunk");
   }

   public void UpdateChunkGroup(Vector3 playerPosition)
   {
      UpdateChunks( playerPosition,maxViewDistance);
   }
}
