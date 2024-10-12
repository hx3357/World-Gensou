using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IChunkFactory
{
    
    public void ProduceChunk(Vector3Int chunkCoord, Material m_chunkMaterial = null, bool m_isForceUpdate = false,object[] SFGParameters = null);

    public void DeleteChunk(Vector3Int m_coord);
    
    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator);
    
    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator, 
        float m_downSampleRate, ComputeShader m_downSampleCS);
    
    public IScalerFieldGenerator GetScalerFieldGenerator();
}


