using System.Collections.Generic;
using UnityEngine;


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
    
    private Vector3Int dotFieldSize;
    protected Vector3Int chunkSize;
    protected Vector3 cellSize;

    private Vector4[] dotField;

    protected Triangle[] triangles;
    protected int triangleCount;
    
    protected IScalerFieldGenerator scalerFieldGenerator;
    protected IScalerFieldDownSampler downSampler;
    
    private float downSampleRate;
    
    protected bool isCulling = false;
    
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
    
    void GenerateMesh()
    {
        chunkMesh = new();
        Vector3[] vertices = new Vector3[triangles.Length*3];
        Vector3[] normals = new Vector3[triangles.Length*3];
        int[] indices = new int[triangles.Length*3];
        for (int i = 0; i < triangles.Length; i++)
        {
            vertices[i*3] = triangles[i].p1;
            vertices[i*3+1] = triangles[i].p2;
            vertices[i*3+2] = triangles[i].p3;
            Vector3 normal = Vector3.Cross(vertices[i*3+1]-vertices[i*3],vertices[i*3+2]-vertices[i*3]);
            normals[i*3] = normal;
            normals[i*3+1] = normal;
            normals[i*3+2] = normal;
            indices[i*3] = i*3;
            indices[i*3+1] = i*3+1;
            indices[i*3+2] = i*3+2;
        }
        chunkMesh.vertices = vertices;
        chunkMesh.triangles = indices;
        chunkMesh.normals = normals;
    }

    protected virtual void GenerateLitMesh()
    {
        Dictionary<Vector3,int> vertexIndexMap = new();
        List<Vector3> vertices = new();
        List<int> indices = new();
        int currentVertexIndex = 0;
        foreach(var triangle in triangles)
        {
            Vector3[] p = {triangle.p1,triangle.p2,triangle.p3};
            foreach(var vertex in p)
            {
                if(!vertexIndexMap.ContainsKey(vertex))
                {
                    vertexIndexMap.Add(vertex,currentVertexIndex);
                    vertices.Add(vertex);
                    currentVertexIndex++;
                }
                indices.Add(vertexIndexMap[vertex]);
            }
        }
        chunkMesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = indices.ToArray()
        };
        chunkMesh.RecalculateNormals();
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
        chunk.dotField = dotField;
        chunk.SetVolume(origin,chunkSize,cellSize);
        chunk.SetMesh(chunkMesh);
        chunk.SetMaterial(chunkMaterial);
        return chunk;
    }
    
    
    protected void PrepareChunkMesh(Vector3 m_origin, Vector3Int m_chunkSize, Vector3 m_cellSize,
        float m_isoSurface, float m_lerpParam)
    {
        origin = m_origin;
        dotFieldSize = new Vector3Int(m_chunkSize.x+1,m_chunkSize.y+1,m_chunkSize.z+1);
        chunkSize =m_chunkSize;
        cellSize = m_cellSize;
        isoSurface = m_isoSurface;
        lerpParam = m_lerpParam;
        cellSize = m_cellSize;

        dotField = scalerFieldGenerator.GenerateDotField(origin, dotFieldSize, cellSize);
        
        if(!isCulling)
        {
            Vector4[] dotFieldDownSampled =  downSampler?.DownSample(dotField,dotFieldSize,cellSize,downSampleRate,
                out dotFieldSize,out chunkSize,out cellSize);
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
        }
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
    protected void PrepareChunkMesh(Vector3 m_origin, Vector3Int m_chunkSize, Vector3 m_cellSize,
        float m_isoSurface, float m_lerpParam,Chunk chunk)
    {
        origin = m_origin;
        dotFieldSize = new Vector3Int(m_chunkSize.x+1,m_chunkSize.y+1,m_chunkSize.z+1);
        chunkSize =m_chunkSize;
        cellSize = m_cellSize;
        isoSurface = m_isoSurface;
        lerpParam = m_lerpParam;
        cellSize = m_cellSize;
        
        if(chunk!=null && chunk.dotField!=null)
        {
            dotField = chunk.dotField;
        }
        else
        {
            dotField = scalerFieldGenerator.GenerateDotField(origin, dotFieldSize, cellSize);
        }

        if(!isCulling)
        {
            Vector4[] dotFieldDownSampled = downSampler?.DownSample(dotField, dotFieldSize, cellSize, downSampleRate,
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
        }
    }

    #endregion

    #region ExposingAPI   

    public virtual Chunk ProduceChunk(Vector3 m_center, Vector3Int m_chunkSize, Vector3 m_cellSize, Material m_chunkMaterial = null)
    {
        PrepareChunkMesh(m_center,m_chunkSize,m_cellSize,isoSurface,lerpParam);
        if(!isCulling)
            GenerateLitMesh();
        chunkMaterial = m_chunkMaterial!=null ? m_chunkMaterial : new Material(Shader.Find("Universal Render Pipeline/Lit"));
        return CreateChunkObject();
    }
    
    public Chunk ProduceChunk(Vector3 m_center,Material m_chunkMaterial=null)
    {
       return ProduceChunk(m_center,IChunkFactory.universalChunkSize,IChunkFactory.universalCellSize,m_chunkMaterial);
    }
    
    public Chunk ProduceChunk(Vector3Int chunkCoord, Chunk.LODLevel lodLevel ,Material m_chunkMaterial = null)
    {
        isCulling = lodLevel == Chunk.LODLevel.Culling;
        SetDownSampler(downSampler,Chunk.lodDownSampleRateTable[lodLevel]);
        Chunk chunk = ProduceChunk(Chunk.GetChunkOriginByCoord(chunkCoord), IChunkFactory.universalChunkSize, 
            IChunkFactory.universalCellSize, m_chunkMaterial);
        chunk.chunkCoord = chunkCoord;
        chunk.SetLODLevel(lodLevel);
        return chunk;
    }
    
    public Chunk ProduceChunk(Vector3Int chunkCoord, Material m_chunkMaterial = null)
    {
        Chunk chunk = ProduceChunk(Chunk.GetChunkOriginByCoord(chunkCoord),IChunkFactory.universalChunkSize, 
            IChunkFactory.universalCellSize, m_chunkMaterial);
        chunk.chunkCoord = chunkCoord;
        return chunk;
    }
    
    public virtual void SetChunk(Chunk chunk,Vector3 m_center,Vector3Int m_chunkSize,
        Vector3 m_cellSize)
    {
        PrepareChunkMesh(m_center,m_chunkSize,m_cellSize,isoSurface,lerpParam);
        GenerateLitMesh();
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
        SetDownSampler(downSampler,(float)lodLevel);
        PrepareChunkMesh(chunk.origin,IChunkFactory.universalChunkSize,IChunkFactory.universalCellSize,isoSurface,lerpParam,chunk);
        GenerateLitMesh();
        chunk.SetLODLevel(lodLevel);
        chunk.SetVolume(origin,chunkSize,cellSize);
        chunk.SetMesh(chunkMesh);
    }
    
    public virtual void SetChunk(Chunk chunk)
    {
        PrepareChunkMesh(chunk.origin,IChunkFactory.universalChunkSize,IChunkFactory.universalCellSize,isoSurface,lerpParam);
        GenerateLitMesh();
        chunk.SetVolume(origin,chunkSize,cellSize);
        chunk.SetMesh(chunkMesh);
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