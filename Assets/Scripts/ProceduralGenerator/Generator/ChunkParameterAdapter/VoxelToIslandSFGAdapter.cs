using System;
using ChunkDispatchers.VoxelBasedDispatch;
using UnityEngine;

public static class VoxelToIslandSFGAdapter
{
    private static float lakePossibility = 1;

    public static void SetParameter(float possibility)
    {
        lakePossibility = possibility;
    }

    public static object[] ConvertToSFGParameters(object[] parameters)
    {
        return Array.ConvertAll(parameters, x =>
        {
            ChunkParameter chunkParameter = (ChunkParameter)x;
            return (object)ToIslandSFGParameter(chunkParameter);
        });
    }

    static SDFIslandSFGParameter ToIslandSFGParameter(ChunkParameter chunkParameter)
    {
        Vector4[] islandPositions = new Vector4[chunkParameter.voxels.Count];
        Vector4[] islandParameters = new Vector4[chunkParameter.voxels.Count];
        for (int i = 0; i < chunkParameter.voxels.Count; i++)
        {
            Voxel currentVoxel = chunkParameter.voxels[i];

            islandPositions[i] = new Vector4(currentVoxel.center.x, currentVoxel.center.y, currentVoxel.center.z,
                currentVoxel.voxelType);
            islandParameters[i] = new Vector4(currentVoxel.worldSize / 2, currentVoxel.worldSize, 0, 0);
        }

        return new SDFIslandSFGParameter(islandPositions, islandParameters);
    }
}