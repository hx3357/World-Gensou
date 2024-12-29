using System;
using ChunkDispatchers.VoxelBasedDispatch;
using UnityEngine;

public static class VoxelToIslandSFGAdapter
{
    public static object[] ConvertToSFGParameters(object[] parameters)
    {
        return Array.ConvertAll(parameters, x =>
        {
            var chunkParameter = (ChunkParameter)x;
            return (object)ToIslandSFGParameter(chunkParameter);
        });
    }

    private static SDFIslandSFGParameter ToIslandSFGParameter(ChunkParameter chunkParameter)
    {
        var islandPositions = new Vector4[chunkParameter.voxels.Count];
        var islandParameters = new Vector4[chunkParameter.voxels.Count];
        for (var i = 0; i < chunkParameter.voxels.Count; i++)
        {
            var currentVoxel = chunkParameter.voxels[i];

            islandPositions[i] = new Vector4(currentVoxel.center.x, currentVoxel.center.y, currentVoxel.center.z,
                currentVoxel.voxelType);
            islandParameters[i] = new Vector4(currentVoxel.worldSize / 2, currentVoxel.worldSize, 0, 0);
        }

        return new SDFIslandSFGParameter(islandPositions, islandParameters);
    }
}