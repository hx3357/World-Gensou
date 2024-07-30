using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUNoiseScalerFieldGenerator : IScalerFieldGenerator
{
    protected object[] parameters;
    protected Vector4[] dotField;
    protected Vector3Int size;
    protected float downSampleRate;
    
    protected CPUNoiseScalerFieldGenerator(params object[] parameters)
    {
        this.parameters = parameters;
    }


    public virtual Vector4[] GenerateDotField( Vector3 m_origin, Vector3Int m_size,
        Vector3 m_cellsize)
    {
        return null;
    }
    
}
