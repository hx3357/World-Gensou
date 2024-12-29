using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IChunkGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="chunkCoord"></param>
    /// <param name="chunkResolution"></param>
    /// <param name="chunkMaterial"></param>
    /// <param name="isForceUpdate">Original chunk will be replaced by the new chunk if this is set to true</param>
    /// <param name="sfgParameters">Specific scalar field generator parameters</param>
    public void ProduceChunk(Vector3Int chunkCoord, int chunkResolution, Material chunkMaterial = null,
        bool isForceUpdate = false, object[] sfgParameters = null);

    public void DeleteChunk(Vector3Int m_coord);

    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator);

    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator,
        float m_downSampleRate, ComputeShader m_downSampleCS);

    public IScalerFieldGenerator GetScalerFieldGenerator();
}