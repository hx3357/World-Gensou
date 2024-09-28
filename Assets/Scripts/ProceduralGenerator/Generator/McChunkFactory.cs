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
using Debug = UnityEngine.Debug;
using Random = Unity.Mathematics.Random;

public class McChunkFactory: MonoBehaviour, IChunkFactory
{
    private Vector3 origin;
    
    private Mesh chunkMesh;
    private Material chunkMaterial;
    
    private const int numofThreads = 8;

    private ComputeShader cs;
    private ComputeShader downSampleCS;
    
    private HashSet<Vector3> currentProducingChunkSet = new HashSet<Vector3>();
    private Dictionary<Vector3,Chunk> chunkDict = new Dictionary<Vector3, Chunk>();
    
    private float isoSurface;
    private float lerpParam;
    
    private Vector3Int dotFieldSize,originDotFieldSize;
    private Vector3Int chunkSize;
    private Vector3 cellSize;

    private Vector4[] dotField;
    
    private IScalerFieldGenerator scalerFieldGenerator;
    
    private float downSampleRate;
    
    private Material defaultChunkMaterial;
    
    
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
        cs.SetVector(ChunkOrigin,m_origin);
        cs.SetVector(CellSize, m_cellSize);
        cs.SetBuffer(kernel,InputPoints,pointBuffer);
        cs.SetBuffer(kernel, OutputTriangles, triangleBuffer);
        cs.Dispatch(kernel, Mathf.CeilToInt(m_dotFieldSize.x / (float)numofThreads), 
            Mathf.CeilToInt(m_dotFieldSize.y / (float)numofThreads), 
            Mathf.CeilToInt(m_dotFieldSize.z / (float)numofThreads));
    }
    
    [BurstCompile]
    struct GenerateMeshJob:IJob
    {
        [ReadOnly]
        public NativeArray<Triangle> triangles;
        public NativeList<float3> vertices;
        public NativeList<Color32> vertColors;
        public NativeList<int> indices;
        public NativeHashMap<float3, int> vertexIndexMap;

        private Color32 ExtractImplColor(int implColor)
        {
            return new Color32((byte)(implColor & 0xFF), (byte)((implColor >> 8) & 0xFF),
                (byte)((implColor >> 16) & 0xFF), (byte)((implColor >> 24) & 0xFF));
        }
        
        private Color32 ExtractImplColor(int implColor,byte a)
        {
            return new Color32((byte)(implColor & 0xFF), (byte)((implColor >> 8) & 0xFF),
                (byte)((implColor >> 16) & 0xFF), a);
        }
        
        public void Execute()
        {
            int currentVertexIndex = 0;
            foreach (var triangle in triangles)
            {
                if (!vertexIndexMap.ContainsKey(triangle.p1))
                {
                    vertexIndexMap.Add(triangle.p1, currentVertexIndex);
                    vertices.Add(triangle.p1);
                    vertColors.Add(ExtractImplColor(triangle.implColor1,255));
                    currentVertexIndex++;
                }
                indices.Add(vertexIndexMap[triangle.p1]);
                if (!vertexIndexMap.ContainsKey(triangle.p2))
                {
                    vertexIndexMap.Add(triangle.p2, currentVertexIndex);
                    vertices.Add(triangle.p2);
                    vertColors.Add(ExtractImplColor(triangle.implColor2,255));
                    currentVertexIndex++;
                }
                indices.Add(vertexIndexMap[triangle.p2]);
                if (!vertexIndexMap.ContainsKey(triangle.p3))
                {
                    vertexIndexMap.Add(triangle.p3, currentVertexIndex);
                    vertices.Add(triangle.p3);
                    vertColors.Add(ExtractImplColor(triangle.implColor3,255));
                    currentVertexIndex++;
                }
                indices.Add(vertexIndexMap[triangle.p3]);
            }
        }
    }
    
    IEnumerator ProduceChunkCoroutine1(Vector3 m_origin, Material m_chunkMaterial = null)
    {
        Vector3 _origin = origin;
        Vector3Int _chunkSize = chunkSize;
        Vector3 _cellSize = cellSize;
        Vector3Int _dotFieldSize = dotFieldSize;
        float _downSampleRate = downSampleRate;
        
        //Generate Dot Field
        ScalerFieldRequestData requestData = scalerFieldGenerator.StartGenerateDotField(_origin, _dotFieldSize, _cellSize);
        Dot[] _dotField = null;
        bool isZeroFlag;
        while(true)
        {
            bool isDone;
            Dot[] __dotField;
            (isDone, __dotField, isZeroFlag) = scalerFieldGenerator.GetState(ref requestData);
            if(__dotField!=null)
                _dotField = __dotField;
            if(isDone)
                break;
            yield return null;
        }
        scalerFieldGenerator.Release(requestData);
        
        if(isZeroFlag)
            yield break;
        
        // chunk.SetDotField(_dotField,_dotFieldSize);
        
        //Down Sample
        // if(_downSampleRate>1)
        // {
        //     IScalerFieldDownSampler downSampler = new GPUTrilinearScalerFieldDownSampler(downSampleCS);
        //     downSampler.StartDownSample(_dotField, _dotFieldSize, _cellSize, _downSampleRate,
        //         out _dotFieldSize, out _chunkSize, out _cellSize);
        //     while(!downSampler.GetState())
        //         yield return null;
        //     _dotField = downSampler.GetDownSampledDotField();
        // }
        
        //Marching Cube
        
        
        var pointBuffer = new ComputeBuffer(_dotFieldSize.x*_dotFieldSize.y*_dotFieldSize.z, Dot.GetSize());
        var triangleBuffer = new ComputeBuffer(5*_dotFieldSize.x*_dotFieldSize.y*_dotFieldSize.z, 
            Triangle.SizeOf, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        var triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        pointBuffer.SetData(_dotField);
        
        RunMarchingCubeComputeShader(_origin, _cellSize,_dotFieldSize,pointBuffer,triangleBuffer);
        
        ComputeBuffer.CopyCount(triangleBuffer,triangleCountBuffer,0);
        AsyncGPUReadbackRequest tribufferRequest = AsyncGPUReadback.Request(triangleBuffer);
        AsyncGPUReadbackRequest tribuffercountRequest = AsyncGPUReadback.Request(triangleCountBuffer,  sizeof(int),0);
        
        int[] count = new int[1];
        NativeArray<Triangle> rawTriangles = new();
        bool isRawTriangleReady = false,isCountReady = false;
        
        while (!tribufferRequest.done||!tribuffercountRequest.done)
        {
            if(tribufferRequest.hasError||tribuffercountRequest.hasError)
            {
                Debug.LogError("GPU Readback Error");
            }
            
            if (tribufferRequest.done&&!isRawTriangleReady)
            {
                rawTriangles = tribufferRequest.GetData<Triangle>();
                isRawTriangleReady = true;
            }
            
            if(tribuffercountRequest.done&&!isCountReady)
            {
                tribuffercountRequest.GetData<int>().CopyTo(count);
                isCountReady = true;
            }
            
            yield return null;
        }
        
        if (!isCountReady)
        {
            tribuffercountRequest.GetData<int>().CopyTo(count);
        }
        
        if (!isRawTriangleReady)
        {
            rawTriangles = tribufferRequest.GetData<Triangle>();
        }
        
        int triangleCount = count[0];
        Triangle[] _triangles = new Triangle[triangleCount];
        try
        {
            //Probably because of the life cycle of the requested data, the native array may have been disposed
            //Simply starting over to reproduce the chunk is the most elegant way so far
            rawTriangles.Slice(0, triangleCount).CopyTo(_triangles);
        }
        catch (ObjectDisposedException e)
        {
            Debug.LogWarning("Retry Produce Chunk");
            rawTriangles.Dispose();
            pointBuffer.Release();
            triangleBuffer.Release();
            triangleCountBuffer.Release();
            currentProducingChunkSet.Remove(_origin);
            ProduceChunk(_origin, _chunkSize, _cellSize, m_chunkMaterial);
            yield break;
        }

        rawTriangles.Dispose();
        pointBuffer.Release();
        triangleBuffer.Release();
        triangleCountBuffer.Release();
        
        //Generate Mesh
        GenerateMeshJob job = new()
        {
            triangles = new NativeArray<Triangle>(_triangles, Allocator.Persistent),
            vertices = new(0, Allocator.Persistent),
            indices = new(0,Allocator.Persistent),
            vertexIndexMap = new(_triangles.Length, Allocator.Persistent)
        };
        JobHandle handle = job.Schedule();
        while (!handle.IsCompleted)
            yield return null;
        handle.Complete();
        chunkMesh = new();
        Vector3[] vertices = new Vector3[job.vertices.Length];
        int[] indices = new int[job.indices.Length];
        job.vertices.AsArray().Reinterpret<Vector3>().CopyTo(vertices);
        job.indices.AsArray().CopyTo(indices);
        job.vertices.Dispose();
        job.indices.Dispose();
        job.triangles.Dispose();
        job.vertexIndexMap.Dispose();
        chunkMesh.vertices = vertices;
        chunkMesh.triangles = indices;
        chunkMesh.RecalculateNormals();
        chunkMesh.RecalculateBounds();
        chunkMesh.RecalculateTangents();
        
        //Create Chunk GameObject
        GameObject chunkObject = new GameObject("Chunk");
        Chunk chunk = chunkObject.AddComponent<Chunk>();
        chunkMaterial = m_chunkMaterial!=null ? m_chunkMaterial :defaultChunkMaterial;
        chunkObject.transform.position = m_origin;
        chunk.SetVolume(_origin,_chunkSize,_cellSize);
        chunk.SetMesh(chunkMesh);
        chunk.SetMaterial(chunkMaterial);
        chunkDict.TryAdd(m_origin, chunk);
        currentProducingChunkSet.Remove(_origin);
    }
    
    IEnumerator ProduceChunkCoroutine(Vector3 m_origin, Material m_chunkMaterial = null)
    {
        Vector3 _origin = origin;
        Vector3Int _chunkSize = chunkSize;
        Vector3 _cellSize = cellSize;
        Vector3Int _dotFieldSize = dotFieldSize;
        float _downSampleRate = downSampleRate;
        
        //Generate Dot Field
        ScalerFieldRequestData requestData = scalerFieldGenerator.StartGenerateDotField(_origin, _dotFieldSize, _cellSize);
        bool isZeroFlag;
        
        while (true)
        {
            bool isDone;
            try
            {
                (isDone, _, isZeroFlag) = scalerFieldGenerator.GetState(ref requestData, true);
            }catch (Exception e)
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
        
        if(isZeroFlag)
        {
            requestData.buffers[0].Release();
            yield break;
        }
        
        //Marching Cube
        var pointBuffer = requestData.buffers[0];
        var triangleBuffer = new ComputeBuffer(5*_dotFieldSize.x*_dotFieldSize.y*_dotFieldSize.z, 
            Triangle.SizeOf, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        var triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        
        RunMarchingCubeComputeShader(_origin, _cellSize,_dotFieldSize,pointBuffer,triangleBuffer);
        
        ComputeBuffer.CopyCount(triangleBuffer,triangleCountBuffer,0);
        AsyncGPUReadbackRequest tribufferRequest = AsyncGPUReadback.Request(triangleBuffer);
        AsyncGPUReadbackRequest tribuffercountRequest = AsyncGPUReadback.Request(triangleCountBuffer,  sizeof(int),0);
        
        int[] count = new int[1];
        NativeArray<Triangle> rawTriangles = new();
        bool isRawTriangleReady = false,isCountReady = false;
        
        while (!tribufferRequest.done||!tribuffercountRequest.done)
        {
            if(tribufferRequest.hasError||tribuffercountRequest.hasError)
            {
                Debug.LogError("GPU Readback Error");
            }
            
            if (tribufferRequest.done&&!isRawTriangleReady)
            {
                rawTriangles = tribufferRequest.GetData<Triangle>();
                isRawTriangleReady = true;
            }
            
            if(tribuffercountRequest.done&&!isCountReady)
            {
                tribuffercountRequest.GetData<int>().CopyTo(count);
                isCountReady = true;
            }
            
            yield return null;
        }
        
        if (!isCountReady)
        {
            tribuffercountRequest.GetData<int>().CopyTo(count);
        }
        
        if (!isRawTriangleReady)
        {
            rawTriangles = tribufferRequest.GetData<Triangle>();
        }
        
        int triangleCount = count[0];
        Triangle[] _triangles = new Triangle[triangleCount];
        try
        {
            //Probably because of the life cycle of the requested data, the native array may have been disposed
            //Simply starting over to reproduce the chunk is the most elegant way so far
            rawTriangles.Slice(0, triangleCount).CopyTo(_triangles);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error '{e.Message}' occured. Retry Produce Chunk");
            rawTriangles.Dispose();
            pointBuffer.Release();
            triangleBuffer.Release();
            triangleCountBuffer.Release();
            currentProducingChunkSet.Remove(_origin);
            ProduceChunk(_origin, _chunkSize, _cellSize, m_chunkMaterial);
            yield break;
        }
        
        rawTriangles.Dispose();
        pointBuffer.Release();
        triangleBuffer.Release();
        triangleCountBuffer.Release();
        
        //Generate Mesh
        GenerateMeshJob job = new()
        {
            triangles = new NativeArray<Triangle>(_triangles, Allocator.Persistent),
            vertices = new(0, Allocator.Persistent),
            indices = new(0,Allocator.Persistent),
            vertexIndexMap = new(_triangles.Length, Allocator.Persistent),
            vertColors = new(0, Allocator.Persistent)
        };
        
        JobHandle handle = job.Schedule();
        
        while (!handle.IsCompleted)
            yield return null;
        
        handle.Complete();
        
        chunkMesh = new();
        Vector3[] vertices = new Vector3[job.vertices.Length];
        int[] indices = new int[job.indices.Length];
        Color32[] vertColors = new Color32[job.vertColors.Length];
        job.vertices.AsArray().Reinterpret<Vector3>().CopyTo(vertices);
        job.indices.AsArray().CopyTo(indices);
        job.vertColors.AsArray().CopyTo(vertColors);
        
        job.vertices.Dispose();
        job.indices.Dispose();
        job.triangles.Dispose();
        job.vertexIndexMap.Dispose();
        job.vertColors.Dispose();
        
        chunkMesh.vertices = vertices;
        chunkMesh.triangles = indices;
        chunkMesh.colors32 = vertColors;
        chunkMesh.RecalculateNormals();
        chunkMesh.RecalculateBounds();
        chunkMesh.RecalculateTangents();
        
        //Create Chunk GameObject
        GameObject chunkObject = new GameObject("Chunk");
        Chunk chunk = chunkObject.AddComponent<Chunk>();
        chunkMaterial = m_chunkMaterial!=null ? m_chunkMaterial :defaultChunkMaterial;
        chunkObject.transform.position = m_origin;
        chunk.SetVolume(_origin,_chunkSize,_cellSize);
        chunk.SetMesh(chunkMesh);
        chunk.SetMaterial(chunkMaterial);
        chunkDict.TryAdd(m_origin, chunk);
        currentProducingChunkSet.Remove(_origin);
    }

    private void Awake()
    {
        defaultChunkMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = Color.white
        };
    }

    #endregion

    #region ExposingAPI   
    
    
    public void ProduceChunk(Vector3 m_origin, Vector3Int m_chunkSize, Vector3 m_cellSize,
        Material m_chunkMaterial = null, bool m_isForceUpdate = false)
    {
        if(currentProducingChunkSet.Contains(m_origin))
        {
            //Debug.Log("Chunk is already producing");
            return;
        }

        if (!m_isForceUpdate && chunkDict.ContainsKey(m_origin))
        {
            //Debug.LogWarning("Chunk already exists");
            return;
        }
        if(Chunk.zombieChunkDict.TryGetValue(m_origin, out var chunk))
        {
            chunk.EnableChunk();
            chunkDict.TryAdd(m_origin,chunk);
            return;
        }
        origin = m_origin;
        chunkSize =m_chunkSize;
        cellSize = m_cellSize;
        originDotFieldSize = dotFieldSize = new Vector3Int(m_chunkSize.x + 1, m_chunkSize.y + 1, m_chunkSize.z + 1);
        currentProducingChunkSet.Add(m_origin);
        StartCoroutine(ProduceChunkCoroutine(m_origin,m_chunkMaterial));
    }
    
    public void DeleteChunk(Vector3 m_origin)
    {
        if(chunkDict.ContainsKey(m_origin))
        {
            Chunk chunk = chunkDict[m_origin];
            chunk.ClearChunk();
            chunkDict.Remove(m_origin);
        }
    }
    
    public void DeleteChunk(Vector3Int m_coord)
    {
        Vector3 m_origin = Chunk.GetChunkOriginByCoord(m_coord);
        if(chunkDict.ContainsKey(m_origin))
        {
            Chunk chunk = chunkDict[m_origin];
            chunk.ClearChunk();
            chunkDict.Remove(m_origin);
        }
    }
    
    public void ProduceChunk(Vector3Int chunkCoord, Chunk.LODLevel lodLevel ,Material m_chunkMaterial = null, bool m_isForceUpdate = false)
    {
        SetDownSampler(downSampleCS,Chunk.lodDownSampleRateTable[lodLevel]);
        ProduceChunk(Chunk.GetChunkOriginByCoord(chunkCoord), IChunkFactory.universalChunkSize, 
            IChunkFactory.universalCellSize, m_chunkMaterial, m_isForceUpdate);
    }
    
    public void ProduceChunk(Vector3Int chunkCoord, Material m_chunkMaterial = null, bool m_isForceUpdate = false)
    {
        ProduceChunk(Chunk.GetChunkOriginByCoord(chunkCoord),IChunkFactory.universalChunkSize, 
            IChunkFactory.universalCellSize, m_chunkMaterial, m_isForceUpdate);
    }
    
    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator)
    {
        this.scalerFieldGenerator = m_scalerFieldGenerator;
        downSampleRate = 1;
    }
    
    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator,
        float m_downSampleRate,ComputeShader m_downSampleCS)
    {
        this.scalerFieldGenerator = m_scalerFieldGenerator;
        SetDownSampler(m_downSampleCS,m_downSampleRate);
    }
    
    public void SetExclusiveParameters(ComputeShader m_cs,float m_isoSurface,float m_lerpParam)
    {
        cs = m_cs;
        isoSurface = m_isoSurface;
        lerpParam = m_lerpParam;
    }
    
    public void SetDownSampler(ComputeShader computeShader,float m_downSampleRate)
    {
        downSampleCS = computeShader;
        downSampleRate = m_downSampleRate;
    }
    
    public IScalerFieldGenerator GetScalerFieldGenerator()
    {
        return scalerFieldGenerator;
    }

    #endregion
    
}