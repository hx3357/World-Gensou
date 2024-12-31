using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;

public class GrassManager : MonoSingleton<GrassManager>, ITerrainObjectManager
{
    public static readonly float grassDensity = 0.3f;
    public bool isGrassEnabled = true;
    public float maxViewDistance = 200;
    public Transform playerTransform;
    public GrassInteractableDetector[] interactableDectors;

    private readonly HashSet<Vector3> visibleGrassSet = new();
    private Vector3 lastPlayerPos;
    private IGrassRenderer grassRenderer;

    protected override void Awake()
    {
        base.Awake();
        interactableDectors = GameObject.FindObjectsOfType<GrassInteractableDetector>();
    }

    private void Start()
    {
        grassRenderer = GrassRenderer.Instance;
    }

    private void Update()
    {
        if (isGrassEnabled)
        {
            List<GrassInteractable> curInteractables = null;
            if (interactableDectors.Length > 1)
            {
                curInteractables = new();
                foreach (var dector in interactableDectors)
                {
                    curInteractables.AddRange(dector.GetInteractableObjects());
                }
            }
            else if(interactableDectors.Length == 1)
            {
                curInteractables = interactableDectors[0].GetInteractableObjects();
            }
            
            grassRenderer.DrawGrass(visibleGrassSet.ToArray(), playerTransform.position, curInteractables?.ToArray());
        }
    }

    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation)
    {
        var distance = Vector3.Distance(worldPosition, playerTransform.position);
        if (distance < maxViewDistance) 
            visibleGrassSet.Add(worldPosition);
    }

    public void UpdateObjects(Vector3 playerPosition)
    {
        // CPU Bottleneck
        var visibleSetRemoveList = new List<Vector3>();
        
        Parallel.ForEach(visibleGrassSet, visiblePos =>
        {
            var distance = Vector3.Distance(visiblePos, playerPosition);
            if (distance > maxViewDistance)
                lock (visibleSetRemoveList)
                {
                    visibleSetRemoveList.Add(visiblePos);
                }
        });
        
        for (var i = 0; i < visibleSetRemoveList.Count; i++) 
            visibleGrassSet.Remove(visibleSetRemoveList[i]);
        
        lastPlayerPos = playerPosition;
    }
    
    
}