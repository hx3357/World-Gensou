using System;
using System.Collections.Generic;
using UnityEngine;

public class GrassManager : MonoSingleton<GrassManager>, ITerrainObjectManager
{
    public static readonly float grassDensity = 0.1f;
    public bool isGrassEnabled = true;
    
    private readonly List<Vector3> visibleGrassSet = new();
    private HashSet<Vector3> invisibleGrassSet = new();
    private float maxViewDistance = 200;
    private float maxExistDistance = 700;
    private Vector3 lastPlayerPos;

    private void Update()
    {
        if (isGrassEnabled)
        {
            GrassRenderer.Instance.DrawGrass(visibleGrassSet.ToArray());
        }   
    }

    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation)
    {
        float distance = Vector3.Distance(worldPosition, lastPlayerPos);
        if (distance < maxViewDistance)
        {
            visibleGrassSet.Add(worldPosition);
        }else if (distance < maxExistDistance)
        {
            invisibleGrassSet.Add(worldPosition);
        }
    }

    public void UpdateObjects(Vector3 playerPosition)
    {
        ITerrainObjectManager.UpdateObjectsFunc(visibleGrassSet,invisibleGrassSet,playerPosition,maxViewDistance,maxExistDistance);
        lastPlayerPos = playerPosition;
        
        if (isGrassEnabled)
        {
            GrassRenderer.Instance.DrawGrass(visibleGrassSet.ToArray());
        } 
    }
    

    // private void OnDrawGizmos()
    // {
    //     foreach (var grass in visibleGrassSet)
    //     {
    //         Gizmos.DrawCube(grass,5 * Vector3.one);
    //     }
    // }
}
