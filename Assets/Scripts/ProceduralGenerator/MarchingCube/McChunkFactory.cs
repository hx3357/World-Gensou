using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;

public class McChunkFactory: MonoBehaviour, IChunkFactory
{
     protected Vector3 origin;
    
     protected Mesh chunkMesh;
     protected Material chunkMaterial;
    
    private const int numofThreads = 8;

    private ComputeShader cs;
    
    private ComputeBuffer pointBuffer;
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer triangleCountBuffer;
    
    protected float isoSurface;
    protected float lerpParam;
    
    private Vector3Int dotFieldSize,originDotFieldSize;
    protected Vector3Int chunkSize;
    protected Vector3 cellSize;

    private Vector4[] dotField;

    protected Triangle[] triangles;
    protected int triangleCount;
    
    protected IScalerFieldGenerator scalerFieldGenerator;
    protected IScalerFieldDownSampler downSampler;
    
    private float downSampleRate;
    
    
    private static readonly int Size = Shader.PropertyToID("size");
    private static readonly int IsoSurface = Shader.PropertyToID("isoSurface");
    private static readonly int LerpParam = Shader.PropertyToID("lerpParam");
    private static readonly int InputPoints = Shader.PropertyToID("inputPoints");
    private static readonly int OutputTriangles = Shader.PropertyToID("outputTriangles");

    #region PrivateField

    void InitBuffer()
    {
        pointBuffer = new ComputeBuffer(dotFieldSize.x*dotFieldSize.y*dotFieldSize.z, sizeof(float)*4);
        triangleBuffer = new ComputeBuffer(5*dotFieldSize.x*dotFieldSize.y*dotFieldSize.z, 
            Triangle.SizeOf, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }
    
    void ReleaseBuffer()
    {
        pointBuffer.Release();
        triangleBuffer.Release();
        triangleCountBuffer.Release();
    }

    void RunMarchingCubeComputeShader()
    {
        int kernel = cs.FindKernel("CSMain");
        cs.SetInts(Size, dotFieldSize.x, dotFieldSize.y, dotFieldSize.z);
        cs.SetFloat(IsoSurface, isoSurface);
        cs.SetFloat(LerpParam, lerpParam);
        cs.SetBuffer(kernel,InputPoints,pointBuffer);
        cs.SetBuffer(kernel, OutputTriangles, triangleBuffer);
        cs.Dispatch(kernel, Mathf.CeilToInt(dotFieldSize.x / (float)numofThreads), 
            Mathf.CeilToInt(dotFieldSize.y / (float)numofThreads), 
            Mathf.CeilToInt(dotFieldSize.z / (float)numofThreads));
    }
    
    [BurstCompile]
    struct GenerateMeshJob:IJob
    {
        [ReadOnly]
        public NativeArray<Triangle> triangles;
        public NativeList<float3> vertices;
        public NativeList<int> indices;
        public NativeHashMap<float3, int> vertexIndexMap;
        
        public void Execute()
        {
            int currentVertexIndex = 0;
            foreach (var triangle in triangles)
            {
                if (!vertexIndexMap.ContainsKey(triangle.p1))
                {
                    vertexIndexMap.Add(triangle.p1, currentVertexIndex);
                    vertices.Add(triangle.p1);
                    currentVertexIndex++;
                }
                indices.Add(vertexIndexMap[triangle.p1]);
                if (!vertexIndexMap.ContainsKey(triangle.p2))
                {
                    vertexIndexMap.Add(triangle.p2, currentVertexIndex);
                    vertices.Add(triangle.p2);
                    currentVertexIndex++;
                }
                indices.Add(vertexIndexMap[triangle.p2]);
                if (!vertexIndexMap.ContainsKey(triangle.p3))
                {
                    vertexIndexMap.Add(triangle.p3, currentVertexIndex);
                    vertices.Add(triangle.p3);
                    currentVertexIndex++;
                }
                indices.Add(vertexIndexMap[triangle.p3]);
            }
        }
    }
    
    IEnumerator DispatchMeshGenerationJobCoroutine(Chunk chunk)
    {
        GenerateMeshJob job = new()
        {
            triangles = new NativeArray<Triangle>(triangles, Allocator.Persistent),
            vertices = new(0, Allocator.Persistent),
            indices = new(0,Allocator.Persistent),
            vertexIndexMap = new(triangles.Length, Allocator.Persistent)
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
        chunk.SetMesh(chunkMesh);
        chunk.ShowMesh();
    }
    
    protected Chunk CreateChunkObject()
    {
        GameObject chunkObject = new GameObject("Chunk")
        {
            transform =
            {
                position = origin
            }
        };
        Chunk chunk = chunkObject.AddComponent<Chunk>();
        chunk.SetVolume(origin,chunkSize,cellSize);
        chunk.SetMesh(chunkMesh);
        chunk.SetMaterial(chunkMaterial);
        return chunk;
    }
    
    /// <summary>
    /// Prepare mesh data for chunk
    /// </summary>
    /// <param name="m_origin"></param>
    /// <param name="m_chunkSize"></param>
    /// <param name="m_cellSize"></param>
    /// <param name="m_isoSurface"></param>
    /// <param name="m_lerpParam"></param>
    /// <param name="chunk">Use the existing chunk dot field data</param>
    /// <param name="isUseChunkDotfieldData"></param>
     bool PrepareChunkMesh(Vector3 m_origin, Vector3Int m_chunkSize, Vector3 m_cellSize,
        float m_isoSurface, float m_lerpParam)
    {
        origin = m_origin;
        originDotFieldSize = dotFieldSize = new Vector3Int(m_chunkSize.x+1,m_chunkSize.y+1,m_chunkSize.z+1);
        chunkSize =m_chunkSize;
        cellSize = m_cellSize;
        isoSurface = m_isoSurface;
        lerpParam = m_lerpParam;
        cellSize = m_cellSize;
        (Vector4[] _dotField,bool isZeroFlag) = scalerFieldGenerator.GenerateDotField(origin, dotFieldSize, cellSize);
        if(isZeroFlag)
        {
            return false;
        }
        dotField = _dotField;
        Vector4[] dotFieldDownSampled = downSampler?.DownSample(_dotField, dotFieldSize, cellSize, downSampleRate,
            out dotFieldSize, out chunkSize, out cellSize);
        InitBuffer();
        pointBuffer.SetData(dotFieldDownSampled);
        RunMarchingCubeComputeShader();
        ComputeBuffer.CopyCount(triangleBuffer,triangleCountBuffer,0);
        int[] count = new int[1];
        triangleCountBuffer.GetData(count);
        triangleCount = count[0];
        triangles = new Triangle[triangleCount];
        triangleBuffer.GetData(triangles, 0, 0, triangleCount);
        ReleaseBuffer();
        return true;
    }

    #endregion

    #region ExposingAPI   

    public virtual Chunk ProduceChunk(Vector3 m_center, Vector3Int m_chunkSize, Vector3 m_cellSize, Material m_chunkMaterial = null)
    {
        bool isGenerateMesh = PrepareChunkMesh(m_center, m_chunkSize, m_cellSize, isoSurface, lerpParam);
        GameObject chunkObject = new GameObject("Chunk");
        Chunk chunk = chunkObject.AddComponent<Chunk>();
        chunk.HideMesh();
        chunkMaterial = m_chunkMaterial!=null ? m_chunkMaterial : new Material(Shader.Find("Universal Render Pipeline/Lit"));
        chunkObject.transform.position = m_center;
        chunk.SetVolume(origin,chunkSize,cellSize);
        chunk.SetMesh(chunkMesh);
        chunk.SetMaterial(chunkMaterial);
        chunk.SetDotField(dotField,originDotFieldSize);
        if(isGenerateMesh)
        {
            chunk.ShowMesh();
            StartCoroutine(DispatchMeshGenerationJobCoroutine(chunk));
        }
        return chunk;
    }
    
    public Chunk ProduceChunk(Vector3 m_center,Material m_chunkMaterial=null)
    {
       return ProduceChunk(m_center,IChunkFactory.universalChunkSize,IChunkFactory.universalCellSize,m_chunkMaterial);
    }
    
    public Chunk ProduceChunk(Vector3Int chunkCoord, Chunk.LODLevel lodLevel ,Material m_chunkMaterial = null)
    {
        SetDownSampler(downSampler,Chunk.lodDownSampleRateTable[lodLevel]);
        Chunk chunk = ProduceChunk(Chunk.GetChunkOriginByCoord(chunkCoord), IChunkFactory.universalChunkSize, 
            IChunkFactory.universalCellSize, m_chunkMaterial);
        if(chunk == null)
            return null;
        chunk.chunkCoord = chunkCoord;
        chunk.SetLODLevel(lodLevel);
        return chunk;
    }
    
    public Chunk ProduceChunk(Vector3Int chunkCoord, Material m_chunkMaterial = null)
    {
        Chunk chunk = ProduceChunk(Chunk.GetChunkOriginByCoord(chunkCoord),IChunkFactory.universalChunkSize, 
            IChunkFactory.universalCellSize, m_chunkMaterial);
        if(chunk == null)
            return null;
        chunk.chunkCoord = chunkCoord;
        return chunk;
    }
    
    public virtual void SetChunk(Chunk chunk,Vector3 m_center,Vector3Int m_chunkSize,
        Vector3 m_cellSize)
    {
        PrepareChunkMesh(m_center,m_chunkSize,m_cellSize,isoSurface,lerpParam);
        StartCoroutine(DispatchMeshGenerationJobCoroutine(chunk));
        chunk.SetVolume(origin,chunkSize,cellSize);
        chunk.SetMesh(chunkMesh);
    }
    
    /// <summary>
    /// Change LOD level based on existing dot field data
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="lodLevel"></param>
    public virtual void SetChunk(Chunk chunk,Chunk.LODLevel lodLevel)
    {
        if(lodLevel == chunk.lodLevel)
            return;
       
        chunk.ShowMesh();
        SetDownSampler(downSampler,Chunk.lodDownSampleRateTable[lodLevel]);
        PrepareChunkMesh(chunk.origin,IChunkFactory.universalChunkSize,IChunkFactory.universalCellSize,isoSurface,lerpParam);
        StartCoroutine(DispatchMeshGenerationJobCoroutine(chunk));
      
        chunk.SetLODLevel(lodLevel);
        chunk.SetMesh(chunkMesh);
    }
    
    public virtual void SetChunk(Chunk chunk)
    {
        PrepareChunkMesh(chunk.origin,IChunkFactory.universalChunkSize,IChunkFactory.universalCellSize,isoSurface,lerpParam);
        StartCoroutine(DispatchMeshGenerationJobCoroutine(chunk));
        chunk.SetVolume(origin,chunkSize,cellSize);
    }
    
    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator)
    {
        this.scalerFieldGenerator = m_scalerFieldGenerator;
    }
    
    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator,
        float m_downSampleRate,IScalerFieldDownSampler m_downSampler)
    {
        this.scalerFieldGenerator = m_scalerFieldGenerator;
        SetDownSampler(m_downSampler,m_downSampleRate);
    }
    
    public void SetExclusiveParameters(ComputeShader m_cs,float m_isoSurface,float m_lerpParam)
    {
        cs = m_cs;
        isoSurface = m_isoSurface;
        lerpParam = m_lerpParam;
    }
    
    public void SetDownSampler(IScalerFieldDownSampler m_downSampler,float m_downSampleRate)
    {
        downSampler = m_downSampler;
        downSampleRate = m_downSampleRate;
    }

    #endregion
    
}