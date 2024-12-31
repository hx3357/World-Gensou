using UnityEngine;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
struct MeshSamplingJob : IJobFor
{
    public NativeArray<float3> vertices;
    public NativeArray<byte> verticesType;
    public NativeArray<int> indices;
    [ReadOnly] public TerrainPlaceableObjectParameter TerrainPlaceableObjectParameter;
    public TerrainPlaceableObjectDataBiltable TerrainPlaceableObjectDataBiltable;
    public float3 meshPosition;

    
    private void PlaceObject(int3 dotTypes, float triangleArea, float3 p1, float3 p2, float3 p3)
    {
        TerrainPlaceableObjectParameter.GetMaxDensityAndDotType(dotTypes, out var dotType, out var density);
        double sampleCountExp = triangleArea * density;
        Unity.Mathematics.Random random = new(math.hash(p1 + p2 + p3));
        var sampleCount = GeneratePoisson(random, sampleCountExp);
        for (var i = 0; i < sampleCount; i++)
        {
            var u = random.NextFloat(1);
            var v = random.NextFloat(1);
            if (u + v > 1)
            {
                u = 1 - u;
                v = 1 - v;
            }

            var position = p1 + u * (p2 - p1) + v * (p3 - p1) + meshPosition;
            TerrainPlaceableObjectDataBiltable.Add(dotType, position);
        }
    }
    
    private int GeneratePoisson(Unity.Mathematics.Random random, double lamada)
    {
        var L = math.exp(-lamada);
        double product = 1;
        var count = 0;
        while (product > L)
        {
            product *= random.NextDouble();
            count++;
        }

        return count - 1;
    }

    public void Execute(int i)
    {
        if (i + 2 >= indices.Length)
        {
            return;
        }

        float3 p1 = vertices[indices[i]];
        float3 p2 = vertices[indices[i + 1]];
        float3 p3 = vertices[indices[i + 2]];
        
    }
}

public class MeshSampler
{
    public Vector3[] SampleMesh(Mesh mesh)
    {
        return default;
    }
}
