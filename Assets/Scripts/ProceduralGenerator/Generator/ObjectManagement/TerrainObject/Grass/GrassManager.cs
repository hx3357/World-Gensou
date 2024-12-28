using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GrassManager : MonoSingleton<GrassManager>, ITerrainObjectManager
{
    public static readonly float grassDensity = 0.3f;
    public bool isGrassEnabled = true;
    
    private readonly List<Vector3> visibleGrassSet = new();
    private float maxViewDistance = 220;
    private Vector3 lastPlayerPos;
    private IGrassRenderer grassRenderer;

    private void Start()
    {
        grassRenderer = GrassRenderer.Instance;
    }

    private void Update()
    {
        if (isGrassEnabled)
        {
            grassRenderer.DrawGrass(visibleGrassSet.ToArray(),lastPlayerPos);
        }   
    }

    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation)
    {
        float distance = Vector3.Distance(worldPosition, lastPlayerPos);
        if (distance < maxViewDistance)
        {
            visibleGrassSet.Add(worldPosition);
        }
    }

    public void UpdateObjects(Vector3 playerPosition)
    {
        ITerrainObjectManager.UpdateObjectsFunc(visibleGrassSet,playerPosition,maxViewDistance);
        lastPlayerPos = playerPosition;
        
        if (isGrassEnabled)
        {
            grassRenderer.DrawGrass(visibleGrassSet.ToArray(),playerPosition);
        } 
    }
    
}
