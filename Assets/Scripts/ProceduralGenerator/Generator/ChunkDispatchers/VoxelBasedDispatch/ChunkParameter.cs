using System.Collections.Generic;
using UnityEngine;

namespace ChunkDispatchers.VoxelBasedDispatch
{
    public class ChunkParameter
    {
        public List<Voxel> voxels = new();
        public HashSet<Vector3> voxelPositions = new();

        public void Add(Voxel voxel)
        {
            if(voxelPositions.Contains(voxel.center))
                return;
            voxels.Add(voxel);
            voxelPositions.Add(voxel.center);
        }
    }
}