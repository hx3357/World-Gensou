using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class McChunkFactory:ChunkFactory
{
    private const int numofThreads = 8;

    private ComputeShader cs;
    
    private ComputeBuffer pointBuffer;
    private ComputeBuffer triangleBuffer;
    
    private float isoSurface;
    private float lerpParam;
    
    private Vector3Int dotFieldSize;
    private Vector3Int chunkSize;
    private Vector3 cellSize;

    private Vector4[] dotField;
    
    private IScalerFieldGenerator scalerFieldGenerator;
    private IScalerFieldDownSampler downSampler;
    
    private float downSampleRate;
    

    public McChunkFactory(ComputeShader cs,IScalerFieldGenerator scalerFieldGenerator)
    {
        this.cs = cs;
        this.scalerFieldGenerator = scalerFieldGenerator;
    }

    public McChunkFactory(ComputeShader cs, IScalerFieldGenerator scalerFieldGenerator,float downSampleRate,IScalerFieldDownSampler downSampler)
    {
        this.cs = cs;
        this.scalerFieldGenerator = scalerFieldGenerator;
        this.downSampler = downSampler;
        this.downSampleRate = downSampleRate;
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
        int triangleCount = triangleBuffer.count;
        Triangle[] triangles = new Triangle[triangleCount];
        triangleBuffer.GetData(triangles,0,0,triangleCount);
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

    void GenerateLitMesh()
    {
        chunkMesh = new();
        int triangleCount = triangleBuffer.count;
        Triangle[] triangles = new Triangle[triangleCount];
        triangleBuffer.GetData(triangles,0,0,triangleCount);
        Dictionary<Vector3,int> vertexIndexMap = new();
        List<Vector3> vertices = new();
        List<int> indices = new();
        for (int i = 0; i < triangles.Length; i++)
        {
            Vector3[] p = {triangles[i].p1,triangles[i].p2,triangles[i].p3};
            for (int j = 0; j < 3; j++)
            {
                if(vertexIndexMap.ContainsKey(p[j]))
                {
                    indices.Add(vertexIndexMap[p[j]]);
                }
                else
                {
                    vertices.Add(p[j]);
                    vertexIndexMap.Add(p[j],vertices.Count-1);
                    indices.Add(vertices.Count-1);
                }
            }
        }
        chunkMesh.vertices = vertices.ToArray();
        chunkMesh.triangles = indices.ToArray();
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
    
    void ProduceChunkMesh(Vector3 m_origin, Vector3Int m_chunkSize, Vector3 m_cellSize,
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
        GenerateLitMesh();
        ReleaseBuffer();
    }

    public override Chunk ProduceChunk(Vector3 m_center,Vector3Int m_chunkSize,Vector3 m_cellSize,float m_isoSurface,float m_lerpParam,Material m_chunkMaterial=null)
    {
        ProduceChunkMesh(m_center,m_chunkSize,m_cellSize,m_isoSurface,m_lerpParam);
        chunkMaterial = chunkMaterial!=null ? m_chunkMaterial : new Material(Shader.Find("Universal Render Pipeline/Lit"));
        return CreateChunkObject();
    }
    
    public override void SetChunkMesh(Chunk chunk,Vector3 m_center,Vector3Int m_chunkSize,Vector3 m_cellSize,float m_isoSurface,float m_lerpParam)
    {
        ProduceChunkMesh(m_center,m_chunkSize,m_cellSize,m_isoSurface,m_lerpParam);
        
        chunk.SetVolume(origin,chunkSize,cellSize);
        chunk.SetMesh(chunkMesh);
    }
    
}