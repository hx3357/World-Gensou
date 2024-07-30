using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScalerFieldDownSampler
{
    public Vector4[] DownSample(Vector4[] dotField,Vector3Int m_dotFieldCount,Vector3 cellSize,float downSampleRate,out Vector3Int m_newDotFieldSize,out Vector3Int m_newChunkSize,out Vector3 m_newCellSize);
}
