using System;
using ChunkDispatchers;
using UnityEngine;
using UnityEngine.Assertions;

public class SDFIslandSFGParameter
{
    public Vector4[] islandPositions;
    // x: radius y: height
    public Vector4[] islandParameters;
    
    public SDFIslandSFGParameter(Vector4[] m_islandPositions, Vector4[] m_islandParameters)
    {
        islandPositions = m_islandPositions;
        islandParameters = m_islandParameters;
    }
    
    public SDFIslandSFGParameter()
    {
        islandPositions = Array.Empty<Vector4>();
        islandParameters = Array.Empty<Vector4>();
    }
    
    public void Merge(SDFIslandSFGParameter sdfIslandSFGParameter)
    {
        Vector4[] newIslandPositions = new Vector4[islandPositions.Length + sdfIslandSFGParameter.islandPositions.Length];
        Vector4[] newIslandParameters = new Vector4[islandParameters.Length + sdfIslandSFGParameter.islandParameters.Length];
        Array.Copy(islandPositions,newIslandPositions,islandPositions.Length);
        Array.Copy(sdfIslandSFGParameter.islandPositions,0,newIslandPositions,islandPositions.Length,sdfIslandSFGParameter.islandPositions.Length);
        Array.Copy(islandParameters,newIslandParameters,islandParameters.Length);
        Array.Copy(sdfIslandSFGParameter.islandParameters,0,newIslandParameters,islandParameters.Length,sdfIslandSFGParameter.islandParameters.Length);
        islandPositions = newIslandPositions;
        islandParameters = newIslandParameters;
    }
}