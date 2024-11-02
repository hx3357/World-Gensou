using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public enum TerrainPlaceableObjectIndice
{
    None,
    Grass
}

// Send data to the job
[BurstCompile]
struct TerrainPlaceableObjectParameter
{
    public float grassDensity;

    public void GetMaxDensityAndDotType(int3 dotTypes, out int dotType, out float density)
    {
        if (dotTypes is { x: 0, y: 0, z: 0 })
        {
            dotType = (int)TerrainPlaceableObjectIndice.None;
            density = 0;
        }

        dotType = (int)TerrainPlaceableObjectIndice.Grass;
        density = grassDensity;
    }
}

// Fetch data from the job
[BurstCompile]
struct TerrainPlaceableObjectDataBiltable
{
    public NativeList<float3> grassPositions;

    public TerrainPlaceableObjectDataBiltable(int capacity)
    {
        grassPositions = new NativeList<float3>(capacity, Allocator.TempJob);
    }

    public void Add(int dotType, float3 position)
    {
        if (dotType == (int)TerrainPlaceableObjectIndice.Grass)
            grassPositions.Add(position);
    }

    public TerrainPlaceableObjectData GetPlaceableObjectData()
    {
        TerrainPlaceableObjectData terrainPlaceableObjectData = new TerrainPlaceableObjectData();
        Vector3[] grassPositionList = new Vector3[grassPositions.Length];
        grassPositions.AsArray().Reinterpret<Vector3>().CopyTo(grassPositionList);
        terrainPlaceableObjectData.grassPositions = grassPositionList;
        return terrainPlaceableObjectData;
    }

    public void Dispose()
    {
        grassPositions.Dispose();
    }
}

class TerrainPlaceableObjectData
{
    public Vector3[] grassPositions;
    
    public void SubmitPlaceableObjectData()
    {
        foreach (var grassPosition in grassPositions)
        {
            ObjectPlacer.Instance.PlaceObject(grassPosition, Vector3.one, Vector3.zero,"Grass");
        }
    }
}