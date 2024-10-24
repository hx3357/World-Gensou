﻿using System;
using UnityEngine;

public class IslandClusterGroup : ChunkGroup
{
    public override void Initialize(IChunkFactory m_chunkFactory, IChunkDispatcher chunkDispatcher,
        int m_maxViewDistance, Material m_chunkMaterial, SurroundBox m_surroundBox,
        int m_seed, params object[] parameters)
    {
        base.Initialize(m_chunkFactory,chunkDispatcher ,m_maxViewDistance, m_chunkMaterial, m_surroundBox, m_seed, parameters);
    }

    protected override void UpdateChunks(Vector3 playerPosition, float m_maxViewDistance)
    {
        
    }

    private void OnDrawGizmos()
    {
        
    }
}
