using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

/// <summary>
/// Used to generate mesh from triangles and gather position of the placeable objects 
/// </summary>
[BurstCompile]
struct GenerateTerrainJob : IJob
{
    [ReadOnly] public NativeArray<Triangle> triangles;
    [ReadOnly] public float3 chunkWorldPosition;
    public NativeList<float3> vertices;
    public NativeList<Color32> vertColors;
    public NativeList<int> indices;
    public NativeHashMap<float3, int> vertexIndexMap;
    
    [ReadOnly] public TerrainPlaceableObjectParameter TerrainPlaceableObjectParameter;
    public TerrainPlaceableObjectDataBiltable TerrainPlaceableObjectDataBiltable;

    private Color32 ExtractImplColor(int implColor)
    {
        return new Color32((byte)(implColor & 0xFF), (byte)((implColor >> 8) & 0xFF),
            (byte)((implColor >> 16) & 0xFF), (byte)((implColor >> 24) & 0xFF));
    }

    private Color32 ExtractImplColor(int implColor, byte a)
    {
        return new Color32((byte)(implColor & 0xFF), (byte)((implColor >> 8) & 0xFF),
            (byte)((implColor >> 16) & 0xFF), a);
    }
    
    private void PlaceObject(int3 dotTypes,float triangleArea,float3 p1,float3 p2,float3 p3)
    {
        TerrainPlaceableObjectParameter.GetMaxDensityAndDotType(dotTypes, out int dotType, out var density);
        double sampleCountExp = triangleArea * density;
        Unity.Mathematics.Random random = new (math.hash(p1 + p2 + p3));
        int sampleCount = GeneratePoisson(random, sampleCountExp);
        for(int i = 0; i < sampleCount; i++)
        {
            float u = random.NextFloat(1);
            float v = random.NextFloat(1);
            if(u+v > 1)
            {
                u = 1 - u;
                v = 1 - v;
            }
            float3 position = p1 + u * (p2 - p1) + v * (p3 - p1) + chunkWorldPosition;
            TerrainPlaceableObjectDataBiltable.Add(dotType, position);
        }
    }

    private int GeneratePoisson(Unity.Mathematics.Random random,double lamada)
    {
        double L = math.exp(-lamada);
        double product = 1;
        int count = 0;
        while (product > L)
        {
            product *= random.NextDouble();
            count++;
        }
        return count - 1;
    }

    public void Execute()
    {
        int currentVertexIndex = 0;
        foreach (var triangle in triangles)
        {
            Color32 color;
            int3 dotTypes = 0;
            if (!vertexIndexMap.ContainsKey(triangle.p1))
            {
                vertexIndexMap.Add(triangle.p1, currentVertexIndex);
                vertices.Add(triangle.p1);
                color = ExtractImplColor(triangle.implColor1);
                vertColors.Add(color);
                dotTypes.x = color.a;
                currentVertexIndex++;
            }

            indices.Add(vertexIndexMap[triangle.p1]);

            if (!vertexIndexMap.ContainsKey(triangle.p2))
            {
                vertexIndexMap.Add(triangle.p2, currentVertexIndex);
                vertices.Add(triangle.p2);
                color = ExtractImplColor(triangle.implColor2);
                vertColors.Add(color);
                dotTypes.y = color.a;
                currentVertexIndex++;
            }

            indices.Add(vertexIndexMap[triangle.p2]);

            if (!vertexIndexMap.ContainsKey(triangle.p3))
            {
                vertexIndexMap.Add(triangle.p3, currentVertexIndex);
                vertices.Add(triangle.p3);
                color = ExtractImplColor(triangle.implColor3);
                vertColors.Add(color);
                dotTypes.z = color.a;
                currentVertexIndex++;
            }

            if (dotTypes is not { x: 0, y: 0, z: 0 })
            {
                float triangleArea = math.length(math.cross(triangle.p2 - triangle.p1, triangle.p3 - triangle.p1)) / 2;
                PlaceObject(dotTypes, triangleArea, triangle.p1, triangle.p2, triangle.p3);
            }
            
            indices.Add(vertexIndexMap[triangle.p3]);
        }
    }

    public void Dispose()
    {
        vertices.Dispose();
        indices.Dispose();
        vertColors.Dispose();
        vertexIndexMap.Dispose();
        triangles.Dispose();
        TerrainPlaceableObjectDataBiltable.Dispose();
    }
}