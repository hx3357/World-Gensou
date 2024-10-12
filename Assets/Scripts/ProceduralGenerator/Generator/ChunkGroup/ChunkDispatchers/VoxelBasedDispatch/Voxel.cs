using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Voxel
{
    Vector3 origin;
    float size;
    Vector3 center;
    
    //The number of sampled dots in the voxel
    public int dotCount{ get; private set;}
    
    private Voxel[] childVoxels;
    
    public bool isLeaf;
    
    public const int MAX_DEPTH = 5;
    
    public Voxel(Vector3 origin, float size,float sampleRate,int depth = 0)
    {
        this.origin = origin;
        this.size = size;
        center = origin + size / 2 * Vector3.one;
        dotCount = PoissonSampler.GetPoissonSampleCount(sampleRate, origin);
        if (dotCount <= 1)
        {
            isLeaf = true;
            if (dotCount == 1)
            {
                //TODO: Generate the chunk inside the voxel
            }
        }
        else
        {
            Spilt(depth);
        }
    }

    void Spilt(int curDepth)
    {
        if(curDepth >= MAX_DEPTH)
            return;
        childVoxels = new Voxel[8];
        for (int i = 0; i < 8; i++)
        {
            Vector3 newOrigin = origin + new Vector3(i & 1, (i & 2) >> 1, (i & 4) >> 2) * size / 2;
            childVoxels[i] = new Voxel(newOrigin, size/2, dotCount / 8f,curDepth+1);
        }
    }
    
    public Voxel[] GetLeafVoxels()
    {
        if (isLeaf)
        {
            if(dotCount == 0)
                return Array.Empty<Voxel>();
            return new Voxel[]{this};
        }
        List<Voxel> leafVoxels = new List<Voxel>();
        foreach (var childVoxel in childVoxels)
        {
            leafVoxels.AddRange(childVoxel.GetLeafVoxels());
        }
        return leafVoxels.ToArray();
    }
    
    public void DrawLeafGizmos()
    {
        if (isLeaf)
        {
            if(dotCount == 0)
                return;
            Gizmos.color = new Color(1,0,0,0.5f);
            Gizmos.DrawWireCube(center, Vector3.one * size);
        }
        else
        {
            if(childVoxels == null)
                return;
            foreach (var childVoxel in childVoxels)
            {
                childVoxel.DrawLeafGizmos();
            }
        }
    }
    
    public void DrawGizmo()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, Vector3.one * size);
        Handles.Label(center, dotCount.ToString());
    }
}
