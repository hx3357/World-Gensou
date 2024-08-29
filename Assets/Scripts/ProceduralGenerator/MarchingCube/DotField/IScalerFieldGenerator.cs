using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScalerFieldGenerator
{
    /// <summary>
    /// Write scalar value to point buffer and return a zero flag to indicate whether the chunk is empty
    /// </summary>
    /// <param name="origin">The origin of the chunk</param>
    /// <param name="dotfieldSize"> Cell count of the chunk</param>
    /// <param name="cellsize">Single cell size</param>
    public ScalerFieldRequestData StartGenerateDotField(Vector3 origin,Vector3Int dotfieldSize,Vector3 cellsize);

    /// <summary>
    /// Only call this before generating a batch of chunks
    /// </summary>
    /// <param name="parameters"></param>
    public void SetParameters(object[] parameters);
    
    public bool GetState(ScalerFieldRequestData scalerFieldRequestData);
    
    public Vector4[] GetDotField(ScalerFieldRequestData scalerFieldRequestData, out bool isEmptyFlag);
    
    
}
