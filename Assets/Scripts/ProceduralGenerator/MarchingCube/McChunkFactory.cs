using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Threading;
using Debug = UnityEngine.Debug;

public class McChunkFactory: MonoBehaviour, IChunkFactory
{
     protected Vector3 origin;
    
     protected Mesh chunkMesh;
     Material chunkMaterial;
    
    private const int numofThreads = 8;

    private ComputeShader cs;
    
    private ComputeBuffer pointBuffer;
    private ComputeBuffer triangleBuffer;
    
    private float isoSurface;
    private float lerpParam;
    
    private Vector3Int dotFieldSize;
    protected Vector3Int chunkSize;
    protected Vector3 cellSize;

    private Vector4[] dotField;

    protected Triangle[] triangles;
    protected int triangleCount;
    
    private IScalerFieldGenerator scalerFieldGenerator;
    private IScalerFieldDownSampler downSampler;
    
    private float downSampleRate;
    
    public void SetParameters(ComputeShader m_cs,IScalerFieldGenerator m_scalerFieldGenerator)
    {
        this.cs = m_cs;
        this.scalerFieldGenerator = m_scalerFieldGenerator;
    }
    
    public void SetParameters(ComputeShader m_cs,IScalerFieldGenerator m_scalerFieldGenerator,
        float m_downSampleRate,IScalerFieldDownSampler m_downSampler)
    {
        this.cs = m_cs;
        this.scalerFieldGenerator = m_scalerFieldGenerator;
        this.downSampler = m_downSampler;
        this.downSampleRate = m_downSampleRate;
    }
    
    public void SetDownSampler(IScalerFieldDownSampler m_downSampler,float m_downSampleRate)
    {
        downSampler = m_downSampler;
        downSampleRate = m_downSampleRate;
    }
    
    public void DrawDotFieldGizmos()
    {
        Utility.ShowDotFieldGizmos(dotFieldSize,dotField);
    }
    
    void InitBuffer()
    {
        pointBuffer = new ComputeBuffer(dotFieldSize.x*dotFieldSize.y*dotFieldSize.z, sizeof(float)*4);
        triangleBuffer = new ComputeBuffer(5*dotFieldSize.x*dotFieldSize.y*dotFieldSize.z, 
            Triangle.SizeOf, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
    }
    
    void ReleaseBuffer()
    {
        pointBuffer.Release();
        triangleBuffer.Release();
    }

    void RunMarchingCubeComputeShader()
    {
        int kernel = cs.FindKernel("CSMain");
        cs.SetInts("size", dotFieldSize.x, dotFieldSize.y, dotFieldSize.z);
        cs.SetFloat("isoSurface", isoSurface);
        cs.SetFloat("lerpParam", lerpParam);
        cs.SetBuffer(kernel,"inputPoints",pointBuffer);
        cs.SetBuffer(kernel, "outputTriangles", triangleBuffer);
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
            if (p[0] == Vector3.zero && p[1] == Vector3.zero && p[2] == Vector3.zero)
                break;
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
    
    Chunk CreateChunkObject()
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
    
    protected void PrepareChunkMesh(Vector3 m_origin, Vector3Int m_chunkSize, Vector3 m_cellSize,
        float m_isoSurface, float m_lerpParam)
    {
        this.origin = m_origin;
        this.dotFieldSize = new Vector3Int(m_chunkSize.x+1,m_chunkSize.y+1,m_chunkSize.z+1);
        this.chunkSize =m_chunkSize;
        this.cellSize = m_cellSize;
        this.isoSurface = m_isoSurface;
        this.lerpParam = m_lerpParam;
        this.cellSize = m_cellSize;
        dotField = scalerFieldGenerator.GenerateDotField(origin,dotFieldSize, cellSize);
        if(downSampler!=null)
            dotField = downSampler.DownSample(dotField,dotFieldSize,cellSize,downSampleRate, out dotFieldSize,out chunkSize,out cellSize);
        InitBuffer();
        pointBuffer.SetData(dotField);
        RunMarchingCubeComputeShader();
        triangleCount = triangleBuffer.count;
        triangles = new Triangle[triangleCount];
        triangleBuffer.GetData(triangles,0,0,triangleCount);
        ReleaseBuffer();
    }

    public Chunk ProduceChunk(Vector3 m_center,Vector3Int m_chunkSize,Vector3 m_cellSize,float m_isoSurface,float m_lerpParam,Material m_chunkMaterial=null)
    {
        PrepareChunkMesh(m_center,m_chunkSize,m_cellSize,m_isoSurface,m_lerpParam);
        GenerateLitMesh();
        chunkMaterial = chunkMaterial!=null ? m_chunkMaterial : new Material(Shader.Find("Universal Render Pipeline/Lit"));
        return CreateChunkObject();
    }
    
    public virtual void SetChunkMesh(Chunk chunk,Vector3 m_center,Vector3Int m_chunkSize,Vector3 m_cellSize,float m_isoSurface,float m_lerpParam)
    {
        PrepareChunkMesh(m_center,m_chunkSize,m_cellSize,m_isoSurface,m_lerpParam);
        GenerateLitMesh();
        chunk.SetVolume(origin,chunkSize,cellSize);
        chunk.SetMesh(chunkMesh);
    }
    
}