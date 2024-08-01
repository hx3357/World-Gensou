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

public class McChunkFactoryV2 : McChunkFactory
{
    
    protected override void GenerateLitMesh()
    {
        Dictionary<Vector3,int> vertexIndexMap = new();
        List<Vector3> vertices = new();
        List<int> indices = new();
        
        int currentVertexIndex = 0;
        foreach (var triangle in triangles)
        {
           Vector3[] p = {triangle.p1,triangle.p2,triangle.p3};
           for(int j = 0; j < 3; j++)
           {
               if (!vertexIndexMap.TryGetValue(p[j], out int index))
               {
                   index = currentVertexIndex++;
                   vertexIndexMap.Add(p[j], index);
                   vertices.Add(p[j]);
               }
               indices.Add(index);
           }
        }
        
        chunkMesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = indices.ToArray()
        };
        chunkMesh.RecalculateNormals();
        chunkMesh.RecalculateBounds();
        chunkMesh.RecalculateTangents();
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
                if(triangle.p1.Equals(float3.zero) && 
                   triangle.p2.Equals(float3.zero) && 
                   triangle.p3.Equals(float3.zero))
                    break;
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

    private Stopwatch sw;
    
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
    }

    public override Chunk ProduceChunk(Vector3 m_center, Vector3Int m_chunkSize, Vector3 m_cellSize, Material m_chunkMaterial = null)
    {
        PrepareChunkMesh(m_center,m_chunkSize,m_cellSize,isoSurface,lerpParam);
        chunkMaterial = m_chunkMaterial!=null ? m_chunkMaterial : new Material(Shader.Find("Universal Render Pipeline/Lit"));
        Chunk chunk = CreateChunkObject();
        StartCoroutine(DispatchMeshGenerationJobCoroutine(chunk));
        return chunk;
    }

    public override void SetChunk(Chunk chunk)
    {
        PrepareChunkMesh(chunk.origin,IChunkFactory.universalChunkSize,IChunkFactory.universalCellSize,isoSurface,lerpParam);
        StartCoroutine(DispatchMeshGenerationJobCoroutine(chunk));
        chunk.SetVolume(origin,chunkSize,cellSize);
    }
}
