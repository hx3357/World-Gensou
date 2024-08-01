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
    [Range(1,8)]
    public float downSampleRate = 2;
    [Header("Debug")]
    public bool isRealtimeUpdate = false;
    public bool isUpdateInTestCoroutine = false;
    public bool isShowDotFieldGizmos = false;
    public bool isDownSample = false;
    public bool isGPU = true;
    public bool isUseV2Generator = true;
    
    private Chunk chunk;
    private IChunkFactory mcChunkFactory;
    private IScalerFieldGenerator scalerFieldGenerator;
    private IScalerFieldDownSampler downSampler;
    
    private Coroutine testCoroutine;

    private void Awake()
    {
        instance = this;
        downSampler = isGPU? new GPUTrilinearScalerFieldDownSampler(downSampleCS):new CPUTrilinearScalerFieldDownSampler();
        scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator_2D
            (ocatves, scale, persistance, lacunarity, seed,Vector2.zero);
        mcChunkFactory = isUseV2Generator? gameObject.AddComponent<McChunkFactoryV2>() :gameObject.AddComponent<McChunkFactory>();
        mcChunkFactory.SetParameters(marchingCubeCS,scalerFieldGenerator,downSampleRate,downSampler);
    }

    // Start is called before the first frame update
    void Start()
    {
        chunk = mcChunkFactory.ProduceChunk(Vector3.zero, chunkSize,  cellSize*Vector3.one, isoSurface, lerpParam);
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
         scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator_2D
             (ocatves, scale, persistance, lacunarity, seed,Vector2.zero);
         mcChunkFactory.SetParameters(marchingCubeCS,scalerFieldGenerator,downSampleRate,downSampler);
         mcChunkFactory.SetChunkMesh(chunk, new Vector3(0, 0, 0), chunkSize, cellSize*Vector3.one, isoSurface, lerpParam);
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
