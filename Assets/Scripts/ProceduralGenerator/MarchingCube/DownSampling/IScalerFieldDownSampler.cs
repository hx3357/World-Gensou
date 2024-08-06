using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScalerFieldDownSampler
{
    public void StartDownSample(Vector4[] dotField,Vector3Int m_dotFieldCount,Vector3 cellSize,float downSampleRate,
        out Vector3Int m_newDotFieldSize,out Vector3Int m_newChunkSize,out Vector3 m_newCellSize);
    
    public bool GetState();
    
    public Vector4[] GetDownSampledDotField();
}
