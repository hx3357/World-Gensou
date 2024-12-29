using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public class ObjectPlacer : MonoSingleton<ObjectPlacer>
{
    public Dictionary<string, ITerrainObjectManager> TerrainObjectManagersTable;
    public ObjectTable objectTable;

    public Transform player;

    private PlaceableObjectPool placeableObjectPool;

    private GameObject parentObj;

    public class PlaceableObject
    {
        public readonly string objectName;
        public float maxViewDistance;
        private bool isActive;

        private GameObject currentGameObject;

        public PlaceableObject(string objectName, GameObject gameObjectPrefab, float maxViewDistance, Transform parent)
        {
            currentGameObject = Instantiate(gameObjectPrefab, parent, true);
            this.maxViewDistance = maxViewDistance;
            this.objectName = objectName;
            SetActive(true);
        }

        public void SetActive(bool active)
        {
            isActive = active;
            currentGameObject.SetActive(active);
        }

        public void SetTransform(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            currentGameObject.transform.position = position;
            currentGameObject.transform.localScale = scale;
            currentGameObject.transform.eulerAngles = rotation;
        }

        public float GetDistance(Vector3 playerPosition)
        {
            return Vector3.Distance(currentGameObject.transform.position, playerPosition);
        }

        public void Destory()
        {
            Destroy(currentGameObject);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        parentObj = new GameObject("PlaceableObjects");
        placeableObjectPool = new PlaceableObjectPool(objectTable, parentObj.transform);
    }

    public void Start()
    {
        TerrainObjectManagersTable = new Dictionary<string, ITerrainObjectManager>()
        {
            { "Grass", GrassManager.Instance }
        };
    }

    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation, string objectName)
    {
        // Terrain object case
        if (TerrainObjectManagersTable.TryGetValue(objectName, out var terrainObjectManager))
        {
            terrainObjectManager.PlaceObject(worldPosition, objectSize, objectRotation);
            return;
        }

        var objViewDistance = objectTable.FindPlaceableObjectDataByName(objectName).viewDistance;
        if (Vector3.Distance(worldPosition, player.position) >
            objViewDistance * Chunk.GetWorldSize()[0])
            return;

        var newPlaceableObject = placeableObjectPool.GetObject(objectName);
        newPlaceableObject.SetTransform(worldPosition, objectSize, objectRotation);
    }

    public void UpdatePlacer(Vector3 playerPosition)
    {
        // Terrain object case
        foreach (var terrainObjectManager in TerrainObjectManagersTable)
            terrainObjectManager.Value.UpdateObjects(playerPosition);

        // Normal object case
        // Disable objects out of range
        List<PlaceableObject> objectsToDisable = new();

        foreach (var obj in placeableObjectPool.activeObjects)
            if (obj.GetDistance(playerPosition) > obj.maxViewDistance * Chunk.GetWorldSize()[0])
                objectsToDisable.Add(obj);

        foreach (var obj in objectsToDisable) placeableObjectPool.DisableObject(obj);
    }
}