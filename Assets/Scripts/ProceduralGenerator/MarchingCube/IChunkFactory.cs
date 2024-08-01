using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IChunkFactory
{
    public static Vector3Int universalChunkSize;
    public static Vector3 universalCellSize;
    
    public Chunk ProduceChunk(Vector3 m_center, Vector3Int m_chunkSize, Vector3 m_cellSize,
        Material m_chunkMaterial = null);
    
    public Chunk ProduceChunk(Vector3 m_center, Material m_chunkMaterial = null);
    
    public Chunk ProduceChunk(Vector3Int chunkCoord, Material m_chunkMaterial = null);
    
    /// <summary>
    /// Produce chunk with LOD level
    /// </summary>
    /// <param name="chunkCoord">Chunk position in chunk coordination</param>
    /// <param name="lodLevel">Target LOD level</param>
    /// <param name="m_chunkMaterial">Chunk Material</param>
    /// <returns></returns>
    public Chunk ProduceChunk(Vector3Int chunkCoord,Chunk.LODLevel lodLevel, Material m_chunkMaterial = null);

    public void SetChunk(Chunk chunk, Vector3 m_center, Vector3Int m_chunkSize,
        Vector3 m_cellSize);
    
    /// <summary>
    /// Set chunk with LOD level
    /// </summary>
    /// <param name="chunk">The chunk to set</param>
    /// <param name="lodLevel">Target LOD level</param>
    public void SetChunk(Chunk chunk,Chunk.LODLevel lodLevel);
    
    public void SetChunk(Chunk chunk);

    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator);

    public void SetParameters(IScalerFieldGenerator m_scalerFieldGenerator, 
        float m_downSampleRate, IScalerFieldDownSampler m_downSampler);
}


