using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = Unity.Mathematics.Random;

/// <summary>
/// A generator that produces terrain chunks and placeable objects using marching cube
/// </summary>
public class TerrainChunkGenerator : MonoBehaviour, IChunkGenerator
{
    private Material chunkMaterial;

    private const int numofThreads = 8;

    private ComputeShader cs;

    private Dictionary<Vector3,ProducingChunkToken> currentProducingChunkSet = new ();
    private Dictionary<Vector3, Chunk> chunkDict = new ();

    private float isoSurface;

    private IScalerFieldGenerator scalerFieldGenerator;

    private Material defaultChunkMaterial;
    
    private readonly TerrainPlaceableObjectParameter _curTerrainPlaceableObjectParameter = new (){grassDensity = GrassManager.grassDensity};


    private static readonly int CellCount = Shader.PropertyToID("cellCount");
    private static readonly int IsoSurface = Shader.PropertyToID("isoSurface");
    private static readonly int InputPoints = Shader.PropertyToID("inputPoints");
    private static readonly int OutputTriangles = Shader.PropertyToID("outputTriangles");
    private static readonly int ChunkOrigin = Shader.PropertyToID("chunkOrigin");
    private static readonly int CellSize = Shader.PropertyToID("cellSize");

    #region PrivateField

    void RunMarchingCubeComputeShader(Vector3 m_origin, Vector3 m_cellSize, Vector3Int m_dotFieldSize,
        ComputeBuffer pointBuffer,
        ComputeBuffer triangleBuffer)
    {
        int kernel = cs.FindKernel("CSMain");
        cs.SetInts(CellCount, m_dotFieldSize.x, m_dotFieldSize.y, m_dotFieldSize.z);
        cs.SetFloat(IsoSurface, isoSurface);
        cs.SetVector(ChunkOrigin, m_origin);
        cs.SetVector(CellSize, m_cellSize);
        cs.SetBuffer(kernel, InputPoints, pointBuffer);
        cs.SetBuffer(kernel, OutputTriangles, triangleBuffer);
        cs.Dispatch(kernel, Mathf.CeilToInt(m_dotFieldSize.x / (float)numofThreads),
            Mathf.CeilToInt(m_dotFieldSize.y / (float)numofThreads),
            Mathf.CeilToInt(m_dotFieldSize.z / (float)numofThreads));
    }
    
    

    // IEnumerator ProduceChunkCoroutineWithDotfieldOnCpu(Vector3 m_origin, Material m_chunkMaterial = null)
    // {
    //     Vector3 _origin = origin;
    //     Vector3Int _chunkSize = chunkResolution;
    //     Vector3 _cellSize = cellSize;
    //     Vector3Int _dotFieldSize = dotFieldSize;
    //     float _downSampleRate = downSampleRate;
    //
    //     //Generate Dot Field
    //     ScalerFieldRequestData requestData =
    //         scalerFieldGenerator.StartGenerateDotField(_origin, _dotFieldSize, _cellSize, parameters);
    //     Dot[] _dotField = null;
    //     bool isZeroFlag;
    //     while (true)
    //     {
    //         bool isDone;
    //         Dot[] __dotField;
    //         (isDone, __dotField, isZeroFlag) = scalerFieldGenerator.GetState(ref requestData);
    //         if (__dotField != null)
    //             _dotField = __dotField;
    //         if (isDone)
    //             break;
    //         yield return null;
    //     }
    //
    //     scalerFieldGenerator.Release(requestData);
    //
    //     if (isZeroFlag)
    //         yield break;
    //
    //     // chunk.SetDotField(_dotField,_dotFieldSize);
    //
    //     //Down Sample
    //     // if(_downSampleRate>1)
    //     // {
    //     //     IScalerFieldDownSampler downSampler = new GPUTrilinearScalerFieldDownSampler(downSampleCS);
    //     //     downSampler.StartDownSample(_dotField, _dotFieldSize, _cellSize, _downSampleRate,
    //     //         out _dotFieldSize, out _chunkSize, out _cellSize);
    //     //     while(!downSampler.GetState())
    //     //         yield return null;
    //     //     _dotField = downSampler.GetDownSampledDotField();
    //     // }
    //
    //     //Marching Cube
    //
    //
    //     var pointBuffer = new ComputeBuffer(_dotFieldSize.x * _dotFieldSize.y * _dotFieldSize.z, Dot.GetSize());
    //     var triangleBuffer = new ComputeBuffer(5 * _dotFieldSize.x * _dotFieldSize.y * _dotFieldSize.z,
    //         Triangle.SizeOf, ComputeBufferType.Append);
    //     triangleBuffer.SetCounterValue(0);
    //     var triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    //     pointBuffer.SetData(_dotField);
    //
    //     RunMarchingCubeComputeShader(_origin, _cellSize, _dotFieldSize, pointBuffer, triangleBuffer);
    //
    //     ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
    //     AsyncGPUReadbackRequest tribufferRequest = AsyncGPUReadback.Request(triangleBuffer);
    //     AsyncGPUReadbackRequest tribuffercountRequest = AsyncGPUReadback.Request(triangleCountBuffer, sizeof(int), 0);
    //
    //     int[] count = new int[1];
    //     NativeArray<Triangle> rawTriangles = new();
    //     bool isRawTriangleReady = false, isCountReady = false;
    //
    //     while (!tribufferRequest.done || !tribuffercountRequest.done)
    //     {
    //         if (tribufferRequest.hasError || tribuffercountRequest.hasError)
    //         {
    //             Debug.LogError("GPU Readback Error");
    //         }
    //
    //         if (tribufferRequest.done && !isRawTriangleReady)
    //         {
    //             rawTriangles = tribufferRequest.GetData<Triangle>();
    //             isRawTriangleReady = true;
    //         }
    //
    //         if (tribuffercountRequest.done && !isCountReady)
    //         {
    //             tribuffercountRequest.GetData<int>().CopyTo(count);
    //             isCountReady = true;
    //         }
    //
    //         yield return null;
    //     }
    //
    //     if (!isCountReady)
    //     {
    //         tribuffercountRequest.GetData<int>().CopyTo(count);
    //     }
    //
    //     if (!isRawTriangleReady)
    //     {
    //         rawTriangles = tribufferRequest.GetData<Triangle>();
    //     }
    //
    //     int triangleCount = count[0];
    //     Triangle[] _triangles = new Triangle[triangleCount];
    //     try
    //     {
    //         //Probably because of the life cycle of the requested data, the native array may have been disposed
    //         //Simply starting over to reproduce the chunk is the most elegant way so far
    //         rawTriangles.Slice(0, triangleCount).CopyTo(_triangles);
    //     }
    //     catch (ObjectDisposedException e)
    //     {
    //         Debug.LogWarning("Retry Produce Chunk");
    //         rawTriangles.Dispose();
    //         pointBuffer.Release();
    //         triangleBuffer.Release();
    //         triangleCountBuffer.Release();
    //         currentProducingChunkSet.Remove(_origin);
    //         ProduceChunk(_origin, _chunkSize, _cellSize, m_chunkMaterial);
    //         yield break;
    //     }
    //
    //     rawTriangles.Dispose();
    //     pointBuffer.Release();
    //     triangleBuffer.Release();
    //     triangleCountBuffer.Release();
    //
    //     //Generate Mesh
    //     GenerateMeshJob job = new()
    //     {
    //         triangles = new NativeArray<Triangle>(_triangles, Allocator.Persistent),
    //         vertices = new(0, Allocator.Persistent),
    //         indices = new(0, Allocator.Persistent),
    //         vertexIndexMap = new(_triangles.Length, Allocator.Persistent)
    //     };
    //     JobHandle handle = job.Schedule();
    //     while (!handle.IsCompleted)
    //         yield return null;
    //     handle.Complete();
    //     chunkMesh = new();
    //     Vector3[] vertices = new Vector3[job.vertices.Length];
    //     int[] indices = new int[job.indices.Length];
    //     job.vertices.AsArray().Reinterpret<Vector3>().CopyTo(vertices);
    //     job.indices.AsArray().CopyTo(indices);
    //     job.vertices.Dispose();
    //     job.indices.Dispose();
    //     job.triangles.Dispose();
    //     job.vertexIndexMap.Dispose();
    //     chunkMesh.vertices = vertices;
    //     chunkMesh.triangles = indices;
    //     chunkMesh.RecalculateNormals();
    //     chunkMesh.RecalculateBounds();
    //     chunkMesh.RecalculateTangents();
    //
    //     //Create Chunk GameObject
    //     GameObject chunkObject = new GameObject("Chunk");
    //     Chunk chunk = chunkObject.AddComponent<Chunk>();
    //     chunkMaterial = m_chunkMaterial != null ? m_chunkMaterial : defaultChunkMaterial;
    //     chunkObject.transform.position = m_origin;
    //     chunk.SetVolume(_origin, _chunkSize, _cellSize);
    //     chunk.SetMesh(chunkMesh);
    //     chunk.SetMaterial(chunkMaterial);
    //     chunkDict.TryAdd(m_origin, chunk);
    //     currentProducingChunkSet.Remove(_origin);
    // }
    
    // chunkResolution = m_chunkResolution;
    // cellSize = m_cellSize;
    // originDotFieldSize = dotFieldSize = new Vector3Int(m_chunkResolution.x + 1, m_chunkResolution.y + 1, m_chunkResolution.z + 1);
    // parameters = m_parameters;

    IEnumerator ProduceChunkCoroutine(Vector3 m_origin, Vector3Int m_chunkResolution, Vector3 m_cellSize,Vector3Int m_dotFieldSize,
        object[] parameters,ProducingChunkToken token,
        Material m_chunkMaterial = null,Chunk editChunk = null)
    {
        Vector3 _origin = m_origin;
        Vector3Int _chunkSize = m_chunkResolution;
        Vector3 _cellSize = m_cellSize;
        Vector3Int _dotFieldSize = m_dotFieldSize;

        //Generate Dot Field
        ScalerFieldRequestData requestData =
            scalerFieldGenerator.StartGenerateDotField(_origin, _dotFieldSize, _cellSize, parameters);

        foreach (var buffer in requestData.buffers)
        {
            token.disposables.Push(buffer);
        }
        
        bool isZeroFlag;

        while (true)
        {
            bool isDone;
            try
            {
                (isDone, _, isZeroFlag) = scalerFieldGenerator.GetState(ref requestData, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error '{e.Message}' occured. Retry Produce Chunk");
                scalerFieldGenerator.Release(requestData, false);
                currentProducingChunkSet.Remove(_origin);
                ProduceChunk(_origin, _chunkSize, _cellSize, m_chunkMaterial);
                yield break;
            }

            if (isDone)
                break;
            yield return null;
        }

        scalerFieldGenerator.Release(requestData, true);

        if (isZeroFlag)
        {
            requestData.buffers[0].Release();
            yield break;
        }

        //Marching Cube
        var pointBuffer = requestData.buffers[0];
        var triangleBuffer = new ComputeBuffer(5 * _dotFieldSize.x * _dotFieldSize.y * _dotFieldSize.z,
            Triangle.SizeOf, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        var triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        
        token.disposables.Push(pointBuffer);
        token.disposables.Push(triangleBuffer);
        token.disposables.Push(triangleCountBuffer);

        RunMarchingCubeComputeShader(_origin, _cellSize, _dotFieldSize, pointBuffer, triangleBuffer);

        ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
        AsyncGPUReadbackRequest tribufferRequest = AsyncGPUReadback.Request(triangleBuffer);
        AsyncGPUReadbackRequest tribuffercountRequest = AsyncGPUReadback.Request(triangleCountBuffer, sizeof(int), 0);

        int[] count = new int[1];
        NativeArray<Triangle> rawTriangles =
            new NativeArray<Triangle>(5 * _dotFieldSize.x * _dotFieldSize.y * _dotFieldSize.z, Allocator.Persistent);
        
        token.disposables.Push(rawTriangles);
        
        bool isRawTriangleReady = false, isCountReady = false;
        bool isError = false;

        while (!tribufferRequest.done || !tribuffercountRequest.done)
        {
            if (tribufferRequest.hasError || tribuffercountRequest.hasError)
            {
                Debug.LogError("GPU Readback Error");
                isError = true;
            }

            if (tribufferRequest.done && !isRawTriangleReady)
            {
                tribufferRequest.GetData<Triangle>().CopyTo(rawTriangles);
                isRawTriangleReady = true;
            }

            if (tribuffercountRequest.done && !isCountReady)
            {
                tribuffercountRequest.GetData<int>().CopyTo(count);
                isCountReady = true;
            }

            yield return null;
        }
        
        Triangle[] _triangles;

        try
        {
            if (!isCountReady)
            {
                tribuffercountRequest.GetData<int>().CopyTo(count);
            }

            if (!isRawTriangleReady)
            {
                tribufferRequest.GetData<Triangle>().CopyTo(rawTriangles);
            }

            var triangleCount = count[0];
            _triangles = new Triangle[triangleCount];

            rawTriangles.Slice(0, triangleCount).CopyTo(_triangles);

            if (isError)
                throw new Exception("GPU Readback Error");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error '{e.Message}' occured. Retry Produce Chunk");
            rawTriangles.Dispose();
            pointBuffer.Release();
            triangleBuffer.Release();
            triangleCountBuffer.Release();
            //currentProducingChunkSet.Remove(_origin);
            ProduceChunk(_origin, _chunkSize, _cellSize, m_chunkMaterial);
            yield break;
        }

        rawTriangles.Dispose();
        pointBuffer.Release();
        triangleBuffer.Release();
        triangleCountBuffer.Release();

        //Generate Mesh
        GenerateTerrainJob job = new()
        {
            chunkWorldPosition = _origin,
            triangles = new NativeArray<Triangle>(_triangles, Allocator.TempJob),
            vertices = new(0, Allocator.TempJob),
            indices = new(0, Allocator.TempJob),
            vertexIndexMap = new(_triangles.Length, Allocator.TempJob),
            TerrainPlaceableObjectParameter = _curTerrainPlaceableObjectParameter,
            vertColors = new(0, Allocator.TempJob),
            TerrainPlaceableObjectDataBiltable = new(0)
        };

        JobHandle handle = job.Schedule();
        token.job = job;
        token.handle = handle;
        
        while (!handle.IsCompleted)
            yield return null;

        handle.Complete();

        Mesh chunkMesh = new();
        Vector3[] vertices = new Vector3[job.vertices.Length];
        int[] indices = new int[job.indices.Length];
        Color32[] vertColors = new Color32[job.vertColors.Length];
        
        job.vertices.AsArray().Reinterpret<Vector3>().CopyTo(vertices);
        job.indices.AsArray().CopyTo(indices);
        job.vertColors.AsArray().CopyTo(vertColors);
        
        ProfilerMarker terrainPlaceableObjectDataMarker = new ProfilerMarker("TerrainPlaceableObjectData");
        
        
        TerrainPlaceableObjectData terrainPlaceableObjectData = job.TerrainPlaceableObjectDataBiltable.GetPlaceableObjectData();
        terrainPlaceableObjectData.SubmitPlaceableObjectData();
        
        job.Dispose();

        chunkMesh.vertices = vertices;
        chunkMesh.triangles = indices;
        chunkMesh.colors32 = vertColors;
        chunkMesh.RecalculateNormals();
        chunkMesh.RecalculateBounds();
        chunkMesh.RecalculateTangents();
        
        if(editChunk != null)
        {
            editChunk.SetMeshAndResolution(chunkMesh,_chunkSize.x);
            currentProducingChunkSet.Remove(_origin);
            yield break;
        }

        //Create Chunk GameObject
        GameObject chunkObject = new GameObject("Chunk")
        {
            isStatic = true
        };
        Chunk chunk = chunkObject.AddComponent<Chunk>();
        chunkMaterial = m_chunkMaterial != null ? m_chunkMaterial : defaultChunkMaterial;
        chunkObject.transform.position = m_origin;
        chunk.SetVolume(_origin, _chunkSize, _cellSize);
        chunk.SetMeshAndResolution(chunkMesh,_chunkSize.x);
        chunk.SetMaterial(chunkMaterial);
        chunk.chunkResolution = _chunkSize.x;
        chunkDict.TryAdd(m_origin, chunk);
        currentProducingChunkSet.Remove(_origin);
    }
    
    class ProducingChunkToken
    {
        public Chunk chunk;
        public Stack<IDisposable> disposables = new();
        public GenerateTerrainJob job;
        public JobHandle handle;
    }

    private void Awake()
    {
        defaultChunkMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = Color.white
        };
    }

    private void OnDestroy()
    {
        foreach (var chunkToken in currentProducingChunkSet.Values)
        {
            foreach (var disposable in chunkToken.disposables)
            {
                if(disposable != null)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (ObjectDisposedException e) { }
                }
            }
            
            chunkToken.handle.Complete();
            try
            {
                chunkToken.job.Dispose();
            }
            catch (ObjectDisposedException e) { }
        }
    }

    #endregion

    #region ExposingAPI

    private void ProduceChunk(Vector3 m_origin, Vector3Int m_chunkResolution, Vector3 m_cellSize,
        Material m_chunkMaterial = null, bool m_isForceUpdate = false, object[] m_parameters = null)
    {
        if (currentProducingChunkSet.ContainsKey(m_origin))
        {
            return;
        }
        
        if (Chunk.zombieChunkDict.TryGetValue(m_origin, out var chunk))
        {
            chunk.EnableChunk();
            chunkDict.TryAdd(m_origin, chunk);
            return;
        }
        
        if(chunkDict.TryGetValue(m_origin, out var curChunk))
        {
            if (curChunk.chunkResolution != m_chunkResolution[0])
            {
                // LOD Switch

                if (!curChunk.TrySetResolution(m_chunkResolution.x))
                {
                    ProducingChunkToken _token = new ProducingChunkToken();
                    currentProducingChunkSet[m_origin] = _token;
                    StartCoroutine(ProduceChunkCoroutine(m_origin,m_chunkResolution,m_cellSize, 
                        new Vector3Int(m_chunkResolution.x + 1, m_chunkResolution.y + 1, m_chunkResolution.z + 1),
                        m_parameters,_token, m_chunkMaterial,editChunk: curChunk));
                }
                return;
            }
            
            if (!m_isForceUpdate)
            {
                return;
            }
        }
        
        ProducingChunkToken token = new ProducingChunkToken();
        currentProducingChunkSet[m_origin] = token;
        StartCoroutine(ProduceChunkCoroutine(m_origin,m_chunkResolution,m_cellSize, 
            new Vector3Int(m_chunkResolution.x + 1, m_chunkResolution.y + 1, m_chunkResolution.z + 1)
            ,m_parameters, token,m_chunkMaterial));
    }

    public void DeleteChunk(Vector3Int m_coord)
    {
        Vector3 m_origin = Chunk.GetChunkOriginByCoord(m_coord);
        DeleteChunk(m_origin);
    }
    
    private void DeleteChunk(Vector3 m_origin)
    {
        if (chunkDict.ContainsKey(m_origin))
        {
            Chunk chunk = chunkDict[m_origin];
            chunk.ClearChunk();
            chunkDict.Remove(m_origin);
        }
    }

    public void ProduceChunk(Vector3Int chunkCoord,int chunkResoulution ,Material m_chunkMaterial = null, bool m_isForceUpdate = false,
        object[] parameters = null)
    {
        ProduceChunk(Chunk.GetChunkOriginByCoord(chunkCoord),chunkResoulution * Vector3Int.one,
            Chunk.GetWorldSize()/chunkResoulution, m_chunkMaterial, m_isForceUpdate, parameters);
    }

    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator)
    {
        this.scalerFieldGenerator = m_scalerFieldGenerator;
    }

    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator,
        float m_downSampleRate, ComputeShader m_downSampleCS)
    {
        this.scalerFieldGenerator = m_scalerFieldGenerator;
    }

    public void SetExclusiveParameters(ComputeShader m_cs, float m_isoSurface, float m_lerpParam)
    {
        cs = m_cs;
        isoSurface = m_isoSurface;
    }
    

    public IScalerFieldGenerator GetScalerFieldGenerator()
    {
        return scalerFieldGenerator;
    }

    #endregion
}