using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFScalerFieldGenerator : IScalerFieldGenerator
{
    
    public virtual ScalerFieldRequestData StartGenerateDotField(Vector3 m_origin, Vector3Int dotfieldSize, Vector3 m_cellsize)
    {
        throw new NotImplementedException();
    }
    
    public virtual bool GetState(ScalerFieldRequestData scalerFieldRequestData)
    {
        throw new NotImplementedException();
    }
    
    public virtual Vector4[] GetDotField(ScalerFieldRequestData scalerFieldRequestData, out bool isEmptyFlag)
    {
        throw new NotImplementedException();
    }
}
