using UnityEngine;

public class SDFIslandClusterScalerFieldGenerator:SDFIslandScalerFieldGenerator
{
    public SDFIslandClusterScalerFieldGenerator(ComputeShader m_cs, int m_seed, float m_isoLevel) : base(m_cs, m_seed, m_isoLevel)
    {
    }

    protected override void SetComputeShaderParameters(ComputeShader m_cs, ScalerFieldRequestData scalerFieldRequestData)
    {
        base.SetComputeShaderParameters(m_cs, scalerFieldRequestData);
        
    }
}
