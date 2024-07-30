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
    public bool isShowDotFieldGizmos = false;
    public bool isDownSample = false;
    public bool isGPU = true;
    
    private Chunk chunk;
    private ChunkFactory mcChunkFactory;
    private IScalerFieldGenerator scalerFieldGenerator;
    private IScalerFieldDownSampler downSampler;

    private void Awake()
    {
        instance = this;
        downSampler = isGPU? new GPUTrilinearScalerFieldDownSampler(downSampleCS):new CPUTrilinearScalerFieldDownSampler();
        scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator_2D
            (ocatves, scale, persistance, lacunarity, seed,Vector2.zero);
        mcChunkFactory = new McChunkFactory(marchingCubeCS,scalerFieldGenerator,downSampleRate,isDownSample? downSampler:null);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Vector4[] testDotField = new Vector4[3*3*3];
        // for(int x=0;x<3;x++)
        // for(int y=0;y<3;y++)
        // for(int z=0;z<3;z++)
        // {
        //     testDotField[Utility.GetBufferIndex(x,y,z,new Vector3Int(3,3,3))] = new Vector4(x,y,z,1);
        // }
        //
        // Vector4 [] downSampledDotField = downSampler.DownSample(testDotField,new Vector3Int(3,3,3),3/2.0f,out Vector3Int newSize,out Vector3 newCellSize);
        //
        chunk = mcChunkFactory.ProduceChunk(Vector3.zero, chunkSize,  cellSize*Vector3.one, isoSurface, lerpParam);
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
         mcChunkFactory = new McChunkFactory(marchingCubeCS,scalerFieldGenerator,downSampleRate,isDownSample? downSampler:null);
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
    }
    
#endif
    
}
