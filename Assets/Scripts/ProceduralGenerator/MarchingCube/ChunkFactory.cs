using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkFactory
{
    protected Vector3 origin;
    
    protected Mesh chunkMesh;
    protected Material chunkMaterial;
    
   
    
    public virtual Chunk ProduceChunk(Vector3 m_center,Vector3Int m_chunkSize,Vector3 m_cellSize,float m_isoSurface,float m_lerpParam,Material m_chunkMaterial=null)
    {
        return null;
    }
    
    public virtual void SetChunkMesh(Chunk chunk,Vector3 m_center,Vector3Int m_chunkSize,Vector3 m_cellSize,float m_isoSurface,float m_lerpParam) { }
}


