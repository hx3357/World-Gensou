using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IChunkFactory
{
    public Chunk ProduceChunk(Vector3 m_center, Vector3Int m_chunkSize, Vector3 m_cellSize, float m_isoSurface,
        float m_lerpParam, Material m_chunkMaterial = null);

    public void SetChunkMesh(Chunk chunk, Vector3 m_center, Vector3Int m_chunkSize, Vector3 m_cellSize,
        float m_isoSurface, float m_lerpParam);

    public void SetParameters(ComputeShader m_cs, IScalerFieldGenerator m_scalerFieldGenerator);

    public void SetParameters(ComputeShader m_cs, IScalerFieldGenerator m_scalerFieldGenerator,
        float m_downSampleRate, IScalerFieldDownSampler m_downSampler);
}


