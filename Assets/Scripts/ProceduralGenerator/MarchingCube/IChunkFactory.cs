using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IChunkFactory
{
    public static Vector3Int universalChunkSize;
    public static Vector3 universalCellSize;
    
    public void ProduceChunk(Vector3 m_origin, Vector3Int m_chunkSize, Vector3 m_cellSize,
        Material m_chunkMaterial = null, bool m_isForceUpdate = false);
    
    public void ProduceChunk(Vector3Int chunkCoord, Material m_chunkMaterial = null, bool m_isForceUpdate = false);
    
    /// <summary>
    /// Produce chunk with LOD level
    /// </summary>
    /// <param name="chunkCoord">Chunk position in chunk coordination</param>
    /// <param name="lodLevel">Target LOD level</param>
    /// <param name="m_chunkMaterial">Chunk Material</param>
    /// <returns></returns>
    public void ProduceChunk(Vector3Int chunkCoord,Chunk.LODLevel lodLevel, Material m_chunkMaterial = null, bool m_isForceUpdate = false);

    public void DeleteChunk(Vector3Int m_coord);
    
    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator);
    
    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator, 
        float m_downSampleRate, ComputeShader m_downSampleCS);
    
    public IScalerFieldGenerator GetScalerFieldGenerator();
}


