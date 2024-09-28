using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MeshGenerator 
{
    public static MeshData GenerateMeshData(Triangle[] triangles)
    {
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
        MeshData meshData = new MeshData(vertices.Count)
        {
            vertices = vertices.ToArray(),
            triangles = indices.ToArray()
        };
        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    
    int trangleIndex;
    
    public MeshData(int vertexCount)
    {
        vertices = new Vector3[vertexCount];
        triangles = new int[vertexCount];
        uvs = new Vector2[vertexCount];
    }
    
    public void AddTriangle(int a,int b,int c)
    {
        triangles[trangleIndex] = a;
        triangles[trangleIndex+1] = b;
        triangles[trangleIndex+2] = c;
        trangleIndex += 3;
    }
}
