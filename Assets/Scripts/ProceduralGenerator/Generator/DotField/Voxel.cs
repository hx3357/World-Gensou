using UnityEngine;

public class VoxelMap
{
    public Vector3 voxelSize;
    public Vector3Int coord;
    
    public VoxelMap(Vector3Int coord, Vector3 voxelSize)
    {
        this.coord = coord;
        this.voxelSize = voxelSize;
    }
    
    public Vector3 GetVoxelOriginByCoord(Vector3Int m_coord)
    {
        return new Vector3(m_coord.x*voxelSize.x,
            m_coord.y*voxelSize.y,
            m_coord.z*voxelSize.z);
    }
    
    public Vector3 GetVoxelCenterByCoord(Vector3Int m_coord)
    {
        return GetVoxelOriginByCoord(m_coord) + new Vector3((voxelSize.x-1)/2,
            (voxelSize.y-1)/2,
            (voxelSize.z-1)/2);
    }
    
    public Vector3Int GetVoxelCoordByPosition(Vector3 position)
    {
        return new Vector3Int(Mathf.FloorToInt(position.x/voxelSize.x),
            Mathf.FloorToInt(position.y/voxelSize.y),
            Mathf.FloorToInt(position.z/voxelSize.z));
    }
}