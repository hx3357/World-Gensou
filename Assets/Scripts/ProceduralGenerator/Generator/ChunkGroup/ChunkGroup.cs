using System;
using System.Collections;
using System.Collections.Generic;
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
   
   protected IChunkGenerator chunkGenerator;
   
   // Chunk coord -> resolution
   protected Dictionary<Vector3Int,int> activeChunks {get; private set;} = new ();
   protected SurroundBox surroundBox;

   private PerlinNoise3D perlinNoise3D;
   
   private int firstTimeChunksNumPerGenerate = 50;
   private int firstTimeChunksGenerationInterval = 1;
   
   private int gameplayChunksNumPerGenerate = 30;
   private int gameplayChunksGenerationInterval = 1;

   private int chunksNumPerGenerate => isFirstTime ? firstTimeChunksNumPerGenerate : gameplayChunksNumPerGenerate;
   private int chunksGenerationInterval => isFirstTime ? firstTimeChunksGenerationInterval : gameplayChunksGenerationInterval;
   
   private bool isFirstTime = true;
   
   private ChunkPatameterAdapter chunkPatameterAdapter;
   

   /// <summary>
   /// 
   /// </summary>
   /// <param name="mChunkGenerator"></param>
   /// <param name="m_chunkDispatcher"></param>
   /// <param name="m_maxViewDistance"></param>
   /// <param name="m_chunkMaterial"></param>
   /// <param name="m_surroundBox"></param>
   /// <param name="m_seed"></param>
   /// <param name="parameters">Parameters for scalar field generator</param>
   public virtual void Initialize(IChunkGenerator mChunkGenerator,
      IChunkDispatcher m_chunkDispatcher,
      int m_maxViewDistance, Material m_chunkMaterial, SurroundBox m_surroundBox, int m_seed,
      params object[] parameters)
   {
      chunkDispatcher = m_chunkDispatcher;
      scalerFieldGenerator = mChunkGenerator.GetScalerFieldGenerator();
      chunkGenerator = mChunkGenerator;
      surroundBox = m_surroundBox ?? SurroundBox.InfiniteSurroundBox;
      maxViewDistance = m_maxViewDistance;
      chunkMaterial = m_chunkMaterial;
      scalerFieldParameters = parameters;
      seed = m_seed;
      perlinNoise3D = new PerlinNoise3D();
      perlinNoise3D.SetRandomSeed(seed);
      chunkPatameterAdapter = new ChunkPatameterAdapter(chunkDispatcher,scalerFieldGenerator);
   }
   
   protected virtual void UpdateChunks(Vector3 playerPosition,float m_maxViewDistance)
   {
      if(chunkDispatcher == null)
         return;
      
      ObjectPlacer.Instance.UpdatePlacer();

      List<(Vector3Int,int)> chunksToGenerate = null;
      List<Vector3Int> chunksToDestroy = null;
      object[] chunkParameters = null;
      
      chunkDispatcher.DispatchChunks(surroundBox,activeChunks, playerPosition,m_maxViewDistance,
         ref chunksToGenerate, ref chunksToDestroy, ref chunkParameters);
      
      //Convert chunk parameters to SFG parameters
      chunkParameters = chunkPatameterAdapter.ConvertToSFGParameters(chunkParameters);
      
      if(chunksToDestroy != null)
         foreach (var chunk in chunksToDestroy)
         {
            activeChunks.Remove(chunk);
            chunkGenerator.DeleteChunk(chunk);
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
   IEnumerator AsyncLoadChunksCoroutine(List<(Vector3Int,int)> chunksToBeProduced, object[] m_parameters = null)
   {
      if(m_parameters != null)
         Assert.IsTrue(chunksToBeProduced.Count == m_parameters.Length,"Parameters count should be equal to chunks count");
      
      float startTime = Time.realtimeSinceStartup;
      
      for(int i=0;i<chunksToBeProduced.Count;)
      {
         for(int j=0;j<chunksNumPerGenerate&&i<chunksToBeProduced.Count;j++)
         {
            activeChunks.TryAdd(chunksToBeProduced[i].Item1,chunksToBeProduced[i].Item2);
            chunkGenerator.ProduceChunk(chunksToBeProduced[i].Item1,chunksToBeProduced[i].Item2 ,m_chunkMaterial: chunkMaterial,
               SFGParameters: m_parameters == null ? scalerFieldParameters :new []{ m_parameters[i]},m_isForceUpdate:false);
            i++;
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
