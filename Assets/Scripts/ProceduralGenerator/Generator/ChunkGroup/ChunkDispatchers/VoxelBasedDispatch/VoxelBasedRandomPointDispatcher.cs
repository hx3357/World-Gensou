using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Suitable for surrounding boxs which size varies greatly
/// When the dot count is greater than 1, the voxel will be cut into 8 sub voxels and distribute its dot count to the sub voxels
/// When the dot count is 1, the chunk will be generated inside the voxel
/// When the dot count is 0, the voxel will be ignored
/// </summary>
public class VoxelBasedRandomPointDispatcher : IChunkDispatcher
{
    readonly VoxelMap baseVoxelMap;
    readonly VoxelMap[] voxelMaps;
    readonly float sampleRate;

    private Dictionary<Vector3Int, Voxel> baseVoxelDictionary = new();
    
    public VoxelBasedRandomPointDispatcher(VoxelMap[] m_voxelMaps,float baseVoxelSizeOffset, float sampleRate = 1f)
    {
        this.voxelMaps = m_voxelMaps;
        this.sampleRate = sampleRate;
        baseVoxelMap = new VoxelMap(m_voxelMaps[^1].voxelSize+baseVoxelSizeOffset);
    }
  
    
    public void DispatchChunks(SurroundBox chunkGroupSurroundBox,HashSet<Vector3Int> activeChunks, Vector3 playerPosition, float maxViewDistance, 
        out List<Vector3Int> chunksToGenerate, out List<Vector3Int> chunksToDestroy, out List<object> chunkParameters)
    {
        chunksToGenerate = new List<Vector3Int>();
        chunksToDestroy = new List<Vector3Int>();
        chunkParameters = null;
        
        Vector3Int _playerVoxelCoord = baseVoxelMap.GetVoxelCoordByPosition(playerPosition);
        int celledMaxViewedVoxelRadius = Mathf.CeilToInt(10*maxViewDistance/baseVoxelMap.voxelSize);
        
        for(int x = -celledMaxViewedVoxelRadius;x<= celledMaxViewedVoxelRadius;x++)
        for(int y = -celledMaxViewedVoxelRadius;y<= celledMaxViewedVoxelRadius;y++)
        for(int z = -celledMaxViewedVoxelRadius;z<=celledMaxViewedVoxelRadius;z++)
        {
            Vector3Int voxelCoord = _playerVoxelCoord + new Vector3Int(x,y,z);
            Vector3 voxelOrigin = baseVoxelMap.GetVoxelOriginByCoord(voxelCoord);
            float distance = Vector3Int.Distance(voxelCoord,_playerVoxelCoord);
            if(distance <= celledMaxViewedVoxelRadius)
            {
                baseVoxelDictionary.TryAdd(voxelCoord, new Voxel(voxelOrigin,baseVoxelMap.voxelSize,sampleRate));
            }
        }

        foreach (var voxelCoord in baseVoxelDictionary.Keys.ToArray())
        {
            float distance = Vector3Int.Distance(voxelCoord,_playerVoxelCoord);
            if(distance>celledMaxViewedVoxelRadius)
            {
                baseVoxelDictionary.Remove(voxelCoord);
            }
        }
            
    }

    public void ShowDebugGizmos()
    {
        foreach (var voxel in baseVoxelDictionary)
        {
            voxel.Value.DrawLeafGizmos();
            //voxel.Value.DrawBorderGizmo();
        }
    }
}
