using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.VisualScripting;

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
                if(triangle.p1.Equals(float3.zero) && triangle.p2.Equals(float3.zero) && triangle.p3.Equals(float3.zero))
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
    
    IEnumerator DispatchMeshGenerationJobCoroutine()
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
        {
            yield return null;
        }
        handle.Complete();
        chunkMesh = new();
        chunkMesh.vertices =new Vector3[job.vertices.Length];
        job.vertices.AsArray().Reinterpret<Vector3>().CopyTo(chunkMesh.vertices);
        chunkMesh.triangles = job.indices.ToArray();
        job.vertices.Dispose();
        job.indices.Dispose();
        job.triangles.Dispose();
        job.vertexIndexMap.Dispose();
        chunkMesh.RecalculateNormals();
        chunkMesh.RecalculateBounds();
        chunkMesh.RecalculateTangents();
    }

    public override void SetChunkMesh(Chunk chunk, Vector3 m_center, Vector3Int m_chunkSize, Vector3 m_cellSize, float m_isoSurface,
        float m_lerpParam)
    {
        PrepareChunkMesh(m_center, m_chunkSize, m_cellSize, m_isoSurface, m_lerpParam);
        StartCoroutine(DispatchMeshGenerationJobCoroutine());
        chunk.SetVolume(origin,chunkSize,cellSize);
    }
}
