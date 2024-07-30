using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScalerFieldGenerator
{
    /// <summary>
    /// Write scalar value to point buffer
    /// </summary>
    /// <param name="pointBuffer"></param>
    /// <param name="origin">The origin of the chunk</param>
    /// <param name="size"> Cell count of the chunk</param>
    /// <param name="cellsize">Single cell size</param>
    /// <param name="downSampler"></param>
    public Vector4[] GenerateDotField(Vector3 origin,Vector3Int size,Vector3 cellsize);
    
    
}
