using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChunkFactoryDebugger : MonoBehaviour
{
    public static ChunkFactoryDebugger instance;
    
    public ComputeShader marchingCubeCS;
    public ComputeShader downSampleCS;
    public Material chunkMaterial;
    [Range(0,1)]
    [Header("Marching Cube")]
    public float isoSurface = 0.5f;
    public float lerpParam = 0;
    public Vector3Int chunkSize;
    public float cellSize = 1;
    [Header("Scaler Field Generator")]
    public int ocatves = 4;
    public float scale = 1;
    [Range(0,1)]
    public float persistance = 0.5f;
    public float lacunarity = 2;
    public int seed;
    public int maxHeight = 20;
    public AnimationCurve heightMapping;
    [Range(1,8)]
    [Header("Down Sample")]
    public float downSampleRate = 2;
    public bool isDownSample = false;
    public bool isGPU = true;
    [Header("Debug")]
    public bool isRealtimeUpdate = false;
    public bool isUpdateInTestCoroutine = false;
    public bool isShowDotFieldGizmos = false;
    public bool isUseV2Generator = true;
    
    private Chunk chunk;
    private IChunkFactory mcChunkFactory;
    private IScalerFieldGenerator scalerFieldGenerator;
    private IScalerFieldDownSampler downSampler;
    
    private Coroutine testCoroutine;

    private void Awake()
    {
        instance = this;
        
        Chunk.SetUniversalChunkSize(chunkSize,cellSize*Vector3.one);
        downSampler = isGPU? new GPUTrilinearScalerFieldDownSampler(downSampleCS):new CPUTrilinearScalerFieldDownSampler();
        scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator_2D
            (ocatves, scale, persistance, lacunarity, seed,maxHeight,heightMapping);
        mcChunkFactory = isUseV2Generator? gameObject.AddComponent<McChunkFactoryV2>() :gameObject.AddComponent<McChunkFactory>();
        mcChunkFactory.SetParameters(scalerFieldGenerator,downSampleRate,downSampler);
        if(mcChunkFactory is McChunkFactory value)
            value.SetExclusiveParameters(marchingCubeCS,isoSurface,lerpParam);
    }

    // Start is called before the first frame update
    void Start()
    {
        chunk = mcChunkFactory.ProduceChunk(new Vector3Int(0,0,0));
        chunk.status = Chunk.ChunkStatus.Visible;
        Chunk chunk1 = mcChunkFactory.ProduceChunk(new Vector3Int(1,0,0));
        chunk1.status = Chunk.ChunkStatus.Visible;
        if(isUpdateInTestCoroutine)
          testCoroutine =  StartCoroutine(TestCoroutine());
    }

    IEnumerator TestCoroutine()
    {
        while (true)
        {
            GenerateChunk();
            yield return new WaitForSeconds(1);
        }
    }

  
    public void OnRegenerateMeshButtonClicked()
    {
       GenerateChunk();
    }

     private void OnDrawGizmos()
     {
         if(isShowDotFieldGizmos)
             ((McChunkFactory) mcChunkFactory).DrawDotFieldGizmos();
     }

     void GenerateChunk()
     {
         Chunk.SetUniversalChunkSize(chunkSize,cellSize*Vector3.one);
         scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator_2D
             (ocatves, scale, persistance, lacunarity, seed,maxHeight,heightMapping);
         mcChunkFactory.SetParameters(scalerFieldGenerator,downSampleRate,downSampler);
         if(mcChunkFactory is McChunkFactory value)
             value.SetExclusiveParameters(marchingCubeCS,isoSurface,lerpParam);
         mcChunkFactory.SetChunk(chunk);
     }
     
#if UNITY_EDITOR
    private void OnValidate()
    {
        if(lacunarity < 1)
            lacunarity = 1;
        if(ocatves < 1)
            ocatves = 1;
        if (!Application.isPlaying||!isRealtimeUpdate)
            return;
        GenerateChunk();
        if(!isUpdateInTestCoroutine&&testCoroutine!=null)
            StopCoroutine(testCoroutine);
    }
    
#endif
    
}
