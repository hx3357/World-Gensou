using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChunkDispatchers.VoxelBasedDispatch
{
    public class ChunkParameter
    {
        public List<Voxel> voxels = new();
        private HashSet<Vector3> voxelPositions = new();

        public Voxel rootVoxel;

        public void Add(Voxel voxel)
        {
            if (voxelPositions.Contains(voxel.center))
                return;
            voxels.Add(voxel);
            voxelPositions.Add(voxel.center);
        }

        public void Remove(Voxel voxel)
        {
            if (voxelPositions.Contains(voxel.center))
            {
                voxelPositions.Remove(voxel.center);
                voxels.Remove(voxel);
            }
        }
    }
}