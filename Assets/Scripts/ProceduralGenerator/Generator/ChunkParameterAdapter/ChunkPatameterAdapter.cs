using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the special case of converting parameters from one type of chunk dispatcher to another
/// </summary>
public class ChunkPatameterAdapter
{
    private delegate object[] ConvertToSFGParametersFunc(object[] parameters);

    private ConvertToSFGParametersFunc convertFunc;

    private readonly Dictionary<(Type, Type), ConvertToSFGParametersFunc> CONVERT_FUNC_MAP =
        new()
        {
            {
                (typeof(ChunkDispatchers.VoxelBasedDispatch.VoxelBasedRandomPointDispatcher),
                    typeof(SDFIslandScalerFieldGenerator)),
                VoxelToIslandSFGAdapter.ConvertToSFGParameters
            }
        };


    public ChunkPatameterAdapter(IChunkDispatcher chunkDispatcher, IScalerFieldGenerator scalerFieldGenerator)
    {
        var chunkDispatcherType = chunkDispatcher.GetType();
        var scalerFieldGeneratorType = scalerFieldGenerator.GetType();
        if (CONVERT_FUNC_MAP.ContainsKey((chunkDispatcherType, scalerFieldGeneratorType)))
        {
            var convertToSFGParametersFunc =
                CONVERT_FUNC_MAP[(chunkDispatcherType, scalerFieldGeneratorType)];
            convertFunc = convertToSFGParametersFunc;
        }
        else
        {
            convertFunc = DefaultToSFGParameters;
        }
    }


    public object[] ConvertToSFGParameters(object[] parameters)
    {
        return convertFunc(parameters);
    }

    private static object[] DefaultToSFGParameters(object[] parameters)
    {
        return parameters;
    }
}