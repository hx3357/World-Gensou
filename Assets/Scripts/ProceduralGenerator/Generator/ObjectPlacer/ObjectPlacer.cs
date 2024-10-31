using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectPlacer : MonoSingleton<ObjectPlacer>
{
    public ObjectTable objectTable;
    public Transform player;

    // Key: objectName, Value: inactive objects
    private Dictionary<string, Queue<PlaceableObject>> objectPool;
  
    private List<PlaceableObject> currentObjects = new();
    private GameObject parentObj;
    
    private class PlaceableObject
    {
        public readonly string objectName;
        private GameObject[] runtimeGameObjectLOD;
        private float[] viewDistance;
        private bool isActive;
        private int currentLOD;

        private GameObject currentGameObject => runtimeGameObjectLOD[currentLOD];
        
        public PlaceableObject(string objectName,GameObject[] gameObjectLOD, float[] viewDistance,Transform parent,Vector3 playerPosition)
        {
            runtimeGameObjectLOD = new GameObject[gameObjectLOD.Length];
            
            for(int i = 0;i<gameObjectLOD.Length;i++)
            {
                runtimeGameObjectLOD[i] = Instantiate(gameObjectLOD[i]);
                runtimeGameObjectLOD[i].transform.parent = parent;
            }
            this.viewDistance = viewDistance;
            this.objectName = objectName;
            SetActive(true);
        }
        
        public void SetActive(bool active)
        {
            isActive = active;
            for(int i = 0; i < runtimeGameObjectLOD.Length; i++)
            {
                runtimeGameObjectLOD[i].SetActive(i == currentLOD && active);
            }
        }
        
        private void SetLOD(int lod)
        {
            currentLOD = lod;
            for(int i = 0; i < runtimeGameObjectLOD.Length; i++)
            {
                runtimeGameObjectLOD[i].SetActive(i == currentLOD && isActive);
            }
        }
        
        public bool SetLOD(Vector3 playerPosition)
        {
            for(int i = 0; i < viewDistance.Length; i++)
            {
                if(Vector3.Distance(currentGameObject.transform.position, playerPosition) < viewDistance[i] * Chunk.GetWorldSize()[0])
                {
                    SetLOD(i);
                    return true;
                }
            }

            return false;
        }
        
        public void SetTransform(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            foreach (var gObj in runtimeGameObjectLOD)
            {
                gObj.transform.position = position;
                gObj.transform.localScale = scale;
                gObj.transform.rotation = Quaternion.Euler(rotation);
            }
        }
        
        public float GetDistance(Vector3 playerPosition)
        {
            return Vector3.Distance(currentGameObject.transform.position, playerPosition);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        
        objectPool = new();
        foreach (var placeableObject in objectTable.placeableObjects)
        {
            objectPool.Add(placeableObject.objectName, new ());
        }
        
        parentObj = new GameObject("PlaceableObjects");
    }

    PlaceableObject GetPlaceableObject(string objectName)
    {
        Assert.IsTrue(objectPool.ContainsKey(objectName),
            "Placeable objects does not contain objectName: " + objectName);
        
        if(objectPool[objectName].Count > 0)
        {
            PlaceableObject obj = objectPool[objectName].Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // If there is no available object in the pool, instantiate a new one
        PlaceableObject newObject = null;
        foreach (var placeableObject in objectTable.placeableObjects)
        {
            if (placeableObject.objectName == objectName)
            {
                newObject = new PlaceableObject(objectName, placeableObject.gameObjectLOD, placeableObject.viewDistance,
                    parentObj.transform,player.position);
                break;
            }
        }
        
        return newObject;
    }

    void DisableObject(PlaceableObject obj)
    {
        obj.SetActive(false);
        objectPool[obj.objectName].Enqueue(obj);
    }


    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation, string objectName)
    {
        foreach (var placeableObject in objectTable.placeableObjects)
        {
            if (placeableObject.objectName != objectName) continue;
            if(Vector3.Distance(worldPosition, player.position) > placeableObject.viewDistance[^1] * Chunk.GetWorldSize()[0])
                return;
        }
        
        PlaceableObject newPlaceableObject = GetPlaceableObject(objectName);
        newPlaceableObject.SetTransform(worldPosition, objectSize, objectRotation);
        newPlaceableObject.SetLOD(player.position);
        currentObjects.Add(newPlaceableObject);
    }

    public void UpdatePlacer()
    {
        List<PlaceableObject> objectsToDisable = new();
        
        foreach (var obj in currentObjects)
        {
            bool isStillThere = obj.SetLOD(player.position);
            if (!isStillThere)
            {
                DisableObject(obj);
                objectsToDisable.Add(obj);
            }
        }
        
        foreach (var obj in objectsToDisable)
        {
            currentObjects.Remove(obj);
        }
    }
}