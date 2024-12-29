using UnityEngine;

namespace ChunkDispatchers.VoxelBasedDispatch
{
    public class VoxelMap
    {
        public Vector3 offset;
        public float voxelSize;

        public VoxelMap(float voxelSize, Vector3 offset = default)
        {
            this.voxelSize = voxelSize;
            this.offset = offset;
        }

        public VoxelMap(int voxelChunkSize)
        {
            voxelSize = voxelChunkSize * Chunk.GetWorldSize()[0];
        }

        public Vector3 GetVoxelOriginByCoord(Vector3Int m_coord)
        {
            return new Vector3(m_coord.x * voxelSize,
                m_coord.y * voxelSize,
                m_coord.z * voxelSize) + offset;
        }


        public Vector3 GetVoxelCenterByCoord(Vector3Int m_coord)
        {
            return GetVoxelOriginByCoord(m_coord) + new Vector3((voxelSize - 1) / 2,
                (voxelSize - 1) / 2,
                (voxelSize - 1) / 2);
        }


        public Vector3Int GetVoxelCoordByPosition(Vector3 position)
        {
            return new Vector3Int(Mathf.FloorToInt((position.x - offset.x) / voxelSize),
                Mathf.FloorToInt((position.y - offset.y) / voxelSize),
                Mathf.FloorToInt((position.z - offset.z) / voxelSize));
        }
    }
}