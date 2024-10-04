using UnityEngine;

public class SDFIslandSFGParameter
{
    public Vector4[] islandPositions;
    public Vector4[] islandParameters;
    
    public SDFIslandSFGParameter(Vector4[] m_islandPositions, Vector4[] m_islandParameters)
    {
        islandPositions = m_islandPositions;
        islandParameters = m_islandParameters;
    }
}