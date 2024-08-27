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
    [Header("Marching Cube")]
    [Range(0,1)]
    public float isoSurface = 0.5f;
    public float lerpParam = 0;
    public Vector3Int chunkSize;
    public float cellSize = 1;
    [Header("Scaler Field Generator")]
    public int ocatves = 4;
    public float scale = 1;
    public Vector3 offset;
    [Range(0,1)]
    public float persistance = 0.5f;
    public float lacunarity = 2;
    public int seed;
    public int maxHeight = 20;
    public AnimationCurve heightMapping;
    [Range(0,1)]
    public float heightOffset;
    [Range(0,1)]
    public float heightScale;
    [Range(1,8)]
    [Header("Down Sample")]
    public float downSampleRate = 2;
    public bool isDownSample = false;
    public bool isGPU = true;
    public Chunk.LODLevel lodLevel = Chunk.LODLevel.High;
    [Header("Debug")]
    public bool isRealtimeUpdate = false;
    public bool isUpdateInTestCoroutine = false;
    public bool isShowDotFieldGizmos = false;
    public bool isUseV2Generator = true;
    
    private Chunk chunk,chunk1;
    private IChunkFactory mcChunkFactory;
    private IScalerFieldGenerator scalerFieldGenerator;
    private IScalerFieldDownSampler downSampler;
    
    private Coroutine testCoroutine;

    private void Awake()
    {
        instance = this;
        
        Chunk.SetUniversalChunkSize(chunkSize,cellSize*Vector3.one);
        downSampler = isGPU? new GPUTrilinearScalerFieldDownSampler(downSampleCS):new CPUTrilinearScalerFieldDownSampler();
        scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator2D
            (ocatves, scale, persistance, lacunarity, seed,maxHeight,heightMapping,heightOffset,heightScale);
        mcChunkFactory = gameObject.AddComponent<McChunkFactory>();
        mcChunkFactory.SetParameters(scalerFieldGenerator,downSampleRate,downSampleCS);
        if(mcChunkFactory is McChunkFactory value)
            value.SetExclusiveParameters(marchingCubeCS,isoSurface,lerpParam);
    }

    // Start is called before the first frame update
    void Start()
    {
          chunk = mcChunkFactory.ProduceChunk(new Vector3Int(0,0,0),Chunk.LODLevel.Culling);
         int scale = 4;
         for(int k = 0;k<scale;k++)
         {
             mcChunkFactory.ProduceChunk(new Vector3Int(1,0,k),(Chunk.LODLevel)k);
         }
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
         if (Application.isPlaying&& isShowDotFieldGizmos)
         {
             chunk.ShowDotFieldGizmo();
             chunk1.ShowDotFieldGizmo();
         }
     }

     void GenerateChunk()
     {
         Chunk.SetUniversalChunkSize(chunkSize,cellSize*Vector3.one);
         scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator2D
             (ocatves, scale, persistance, lacunarity, seed,maxHeight,heightMapping,heightOffset,heightScale);
         mcChunkFactory?.SetParameters(scalerFieldGenerator,downSampleRate,downSampleCS);
         if(mcChunkFactory is McChunkFactory value)
             value.SetExclusiveParameters(marchingCubeCS,isoSurface,lerpParam);
         if(chunk!=null)
         {
              mcChunkFactory?.SetChunk(chunk,lodLevel);
             // chunk.gameObject.SetActive(false);
             // chunk = mcChunkFactory.ProduceChunk(offset);
         }
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
