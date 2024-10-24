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
   
   private IChunkDispatcher chunkDispatcher;
   
   private IScalerFieldGenerator scalerFieldGenerator;
   protected object[] scalerFieldParameters;
   
   protected IChunkFactory chunkFactory;
   protected HashSet<Vector3Int> activeChunks {get; private set;} = new ();
   protected SurroundBox surroundBox;

   protected PerlinNoise3D perlinNoise3D;
   
   private int firstTimeChunksNumPerGenerate = 50;
   private int firstTimeChunksGenerationInterval = 1;
   
   private int gameplayChunksNumPerGenerate = 10;
   private int gameplayChunksGenerationInterval = 2;

   private int chunksNumPerGenerate => isFirstTime ? firstTimeChunksNumPerGenerate : gameplayChunksNumPerGenerate;
   private int chunksGenerationInterval => isFirstTime ? firstTimeChunksGenerationInterval : gameplayChunksGenerationInterval;
   
   private bool isFirstTime = true;

   /// <summary>
   /// 
   /// </summary>
   /// <param name="m_chunkFactory"></param>
   /// <param name="m_chunkDispatcher"></param>
   /// <param name="m_maxViewDistance"></param>
   /// <param name="m_chunkMaterial"></param>
   /// <param name="m_surroundBox"></param>
   /// <param name="m_seed"></param>
   /// <param name="parameters">Parameters for scalar field generator</param>
   /// <param name="chunkDispatcher"></param>
   public virtual void Initialize(IChunkFactory m_chunkFactory,
      IChunkDispatcher m_chunkDispatcher,
      int m_maxViewDistance, Material m_chunkMaterial, SurroundBox m_surroundBox, int m_seed,
      params object[] parameters)
   {
      chunkDispatcher = m_chunkDispatcher;
      scalerFieldGenerator = m_chunkFactory.GetScalerFieldGenerator();
      chunkFactory = m_chunkFactory;
      surroundBox = m_surroundBox ?? SurroundBox.InfiniteSurroundBox;
      maxViewDistance = m_maxViewDistance;
      chunkMaterial = m_chunkMaterial;
      scalerFieldParameters = parameters;
      seed = m_seed;
      perlinNoise3D = new PerlinNoise3D();
      perlinNoise3D.SetRandomSeed(seed);
   }
   
   protected virtual void UpdateChunks(Vector3 playerPosition,float m_maxViewDistance)
   {
      if(chunkDispatcher == null)
         return;
      
      chunkDispatcher.DispatchChunks(surroundBox,activeChunks, playerPosition,m_maxViewDistance,
         out List<Vector3Int> chunksToGenerate, out List<Vector3Int> chunksToDestroy, out List<object> chunkParameters);

      

      foreach (var chunk in chunksToDestroy)
      {
         activeChunks.Remove(chunk);
         chunkFactory.DeleteChunk(chunk);
      }
      
      StartCoroutine(AsyncLoadChunksCoroutine(chunksToGenerate,chunkParameters));
   }
   
   /// <summary>
   /// 
   /// </summary>
   /// <param name="chunksToBeProduced"></param>
   /// <param name="m_parameters">A list contains chunk exclusive SFG paramaters if is not null. 
   /// When this parameter is null, the scaler field generator will use the initial parameters
   /// </param>
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
            chunkFactory.ProduceChunk(chunksToBeProduced[i],m_chunkMaterial: chunkMaterial,
               SFGParameters: m_parameters == null ? scalerFieldParameters :new []{ m_parameters[i++]},m_isForceUpdate:true);
         }
         for(int j=0;j<chunksGenerationInterval;j++)
            yield return null;
      }
      
      float duration = Time.realtimeSinceStartup - startTime;
      Debug.Log($"Generate {chunksToBeProduced.Count} chunks in {duration} seconds\n " +
                $"Average: {duration/chunksToBeProduced.Count} seconds per chunk");
      if (isFirstTime)
      {
         isFirstTime = false;
      }
   }

   public void UpdateChunkGroup(Vector3 playerPosition)
   {
      UpdateChunks( playerPosition,maxViewDistance);
   }

   private void OnDrawGizmos()
   {
      chunkDispatcher.ShowDebugGizmos();
   }
}
