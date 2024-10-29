using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the special case of converting parameters from one type of chunk dispatcher to another
/// </summary>
public class ChunkPatameterAdapter
{
    private delegate object[] ConvertToSFGParametersFunc(object[] parameters);
    
    private ConvertToSFGParametersFunc convertFunc;

    private readonly Dictionary<(Type, Type), ConvertToSFGParametersFunc> convertToSFGParametersFuncMap =
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
        Type chunkDispatcherType = chunkDispatcher.GetType();
        Type scalerFieldGeneratorType = scalerFieldGenerator.GetType();
        if (convertToSFGParametersFuncMap.ContainsKey((chunkDispatcherType, scalerFieldGeneratorType)))
        {
            ConvertToSFGParametersFunc convertToSFGParametersFunc =
                convertToSFGParametersFuncMap[(chunkDispatcherType, scalerFieldGeneratorType)];
            convertFunc = convertToSFGParametersFunc;
        }
        else
        {
            convertFunc = DefaultToSFGParameters;
        }
    }
    
    
    public object[] ConvertToSFGParameters(object[] parameters) {
        return convertFunc(parameters);
    }
    
    private static object[] DefaultToSFGParameters(object[] parameters)
    {
        return parameters;
    }
    
}
