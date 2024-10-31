using System;
using System.Collections;
using System.Collections.Generic;
using ChunkDispatchers.VoxelBasedDispatch;
using UnityEngine;

public class ChunkGroupDispatcher : MonoBehaviour
{
    public Transform playerTransform;

    public int maxViewDistance = 5;
    public float chunkSize = 32*4;
    public float cellSize = 1;
    public Material chunkMaterial;
    public float downSampleRate = 1;

    [Header("Marching Cube")] [Range(0, 1)]
    public float isoSurface = 0.5f;

    public float lerpParam = 0;
    [Header("Scaler Field Generator")] public int ocatves = 4;
    public float scale = 40;
    [Range(0, 1)] public float persistance = 0.5f;
    public float lacunarity = 2;
    public int seed = 114514;
    public int maxHeight = 40;
    public AnimationCurve heightMapping;
    [Range(0, 1)] public float heightOffset = 0.446f;
    [Range(0, 1)] public float heightScale = 0.261f;
    [Header("Compute Shader")] public ComputeShader marchingCubeCS;
    public ComputeShader downSampleCS;
    public ComputeShader perlinNoise3DCS;
    public ComputeShader sdfCS;

    private Vector3Int playerChunkCoord, lastPlayerChunkCoord;

    private HashSet<Vector3Int> activeChunks = new HashSet<Vector3Int>();

    private List<ChunkGroup> chunkGroups = new List<ChunkGroup>();

    private void Awake()
    {
        Chunk.SetUniversalChunkSize(chunkSize);
    }

    private void Start()
    {
        Initialize();

        PerlinNoise3D perlinNoise3D = new PerlinNoise3D();
        perlinNoise3D.SetRandomSeed(seed);
    }

    private void Update()
    {
        Vector3 playerPosition = playerTransform.position;
        playerChunkCoord = Chunk.GetChunkCoordByPosition(playerPosition);
        if (playerChunkCoord != lastPlayerChunkCoord)
        {
            UpdateAllChunkGroups(playerPosition);
            lastPlayerChunkCoord = playerChunkCoord;
        }
    }

    void UpdateAllChunkGroups(Vector3 playerPosition)
    {
        foreach (var chunkGroup in chunkGroups)
        {
            chunkGroup.UpdateChunkGroup(playerPosition);
        }
    }

    void Initialize()
    {
        //Set up the down sampler
        IScalerFieldDownSampler downSampler = new GPUTrilinearScalerFieldDownSampler(downSampleCS);

        //Set up the scaler field generator

        IScalerFieldGenerator perlinNoiseScalerFieldGenerator = new PerlinNoiseScalerFieldGenerator2D
            (ocatves, scale, persistance, lacunarity, seed, maxHeight, heightMapping, heightOffset, heightScale);

        //scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator3D(seed,scale,isoSurface);

        //scalerFieldGenerator = new PerlinNoiseScalerFieldGenerator3D_GPU(perlinNoise3DCS,seed,scale,isoSurface);

        IScalerFieldGenerator sdfIslandScalerFieldGenerator =
            new SDFIslandScalerFieldGenerator(sdfCS, seed, isoSurface);

        //Set up the chunk factory
        IChunkFactory chunkFactory0 = gameObject.AddComponent<McChunkFactory>();
        chunkFactory0.SetParameters(sdfIslandScalerFieldGenerator, downSampleRate, downSampleCS);
        if (chunkFactory0 is McChunkFactory value0)
            value0.SetExclusiveParameters(marchingCubeCS, isoSurface, lerpParam);

        IChunkFactory chunkFactory1 = gameObject.AddComponent<McChunkFactory>();
        chunkFactory1.SetParameters(perlinNoiseScalerFieldGenerator, downSampleRate, downSampleCS);
        if (chunkFactory1 is McChunkFactory value1)
            value1.SetExclusiveParameters(marchingCubeCS, isoSurface, lerpParam);

        ChunkGroup chunkGroup0, chunkGroup1;

        chunkGroup0 = gameObject.AddComponent<ChunkGroup>();
        chunkGroup0.Initialize(chunkFactory0,
            new SphericalDispatcher(31), maxViewDistance, chunkMaterial,
            SurroundBox.InfiniteSurroundBox, seed,10 ,new SDFIslandSFGParameter(
                new[] { new Vector4(300, 200, 100, 0), new Vector4(-100, 100, 100, 0) },
                new[] { new Vector4(100, 500, 100, 0), new Vector4(100, 500, 100, 0) }
            ));
        
        

        chunkGroup1 = gameObject.AddComponent<ChunkGroup>();
        chunkGroup1.Initialize(chunkFactory0,new VoxelBasedRandomPointDispatcher(m_onVoxelGenerated: currentVoxel =>
            {
                int islandType = 0;
                float lakePossibility = 1f;
        
                if (currentVoxel.isRoot)
                {
                    if (Mathf.Abs(currentVoxel.center.GetHashCode() % 10000 / 10000f) < lakePossibility)
                    {
                        // Generate lake island
                        islandType = 2;
                        ObjectPlacer.Instance.PlaceObject(
                            currentVoxel.center - currentVoxel.worldSize * 0.1f * Vector3.up,
                            new Vector3(currentVoxel.worldSize * 0.7f, currentVoxel.worldSize / 4 * 0.3f,
                                currentVoxel.worldSize * 0.7f),
                            Vector3.zero, "Lake");
                    }
                }else if (currentVoxel.depth >= 2)
                {
                    if(currentVoxel.center.GetHashCode()%10000/10000f < 0.01f)
                        islandType = 3;
                    else
                    // Generate Upper Island
                    islandType = ((currentVoxel.voxelIndice & 4) >> 2) == 1 ? 1 : islandType;
                }
        
                currentVoxel.voxelType = islandType;
            })
           ,
            maxViewDistance, chunkMaterial, null, seed, 10,new SDFIslandSFGParameter(
                new[] { new Vector4(300, 100, 100, 0), new Vector4(-300, 100, 100, 0) },
                new[] { new Vector4(100, 100, 100, 0), new Vector4(100, 100, 100, 0) }
            ));
        //chunkGroups.Add(chunkGroup0);
        chunkGroups.Add(chunkGroup1);
    }
}