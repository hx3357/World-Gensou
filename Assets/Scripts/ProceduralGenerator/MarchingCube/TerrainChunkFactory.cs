using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkFactory : ChunkFactory
{
    public override Chunk ProduceChunk(Vector3 m_center, Vector3Int m_chunkSize, Vector3 m_cellSize, float m_isoSurface, float m_lerpParam,
        Material m_chunkMaterial = null)
    {
        return base.ProduceChunk(m_center, m_chunkSize, m_cellSize, m_isoSurface, m_lerpParam, m_chunkMaterial);
    }
    
    public override void SetChunkMesh(Chunk chunk, Vector3 m_center, Vector3Int m_chunkSize, Vector3 m_cellSize, float m_isoSurface, float m_lerpParam)
    {
        base.SetChunkMesh(chunk, m_center, m_chunkSize, m_cellSize, m_isoSurface, m_lerpParam);
    }
}
