using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectPlacer : MonoSingleton<ObjectPlacer>
{
    public ObjectTable objectTable;

    private Dictionary<string, List<PlaceableObject>> objectPool;
    private List<PlaceableObject> currentObjects = new();
    
    private class PlaceableObject
    {
        public string objectName;
        public GameObject[] runtimeGameObjectLOD;
        public float[] viewDistance;
        public bool isActive;
        public int currentLOD;
        public GameObject currentGameObject => runtimeGameObjectLOD[currentLOD];
        
        public PlaceableObject(string objectName,GameObject[] runtimeGameObjectLOD, float[] viewDistance)
        {
            for(int i = 0;i<runtimeGameObjectLOD.Length;i++)
            {
                runtimeGameObjectLOD[i] = Instantiate(runtimeGameObjectLOD[i]);
            }
            this.viewDistance = viewDistance;
            isActive = false;
            currentLOD = 0;
            this.objectName = objectName;
        }
        
        public void SetActive(bool active)
        {
            isActive = active;
            for(int i = 0; i < runtimeGameObjectLOD.Length; i++)
            {
                runtimeGameObjectLOD[i].SetActive(i == currentLOD && active);
            }
        }
        
        public void SetLOD(int lod)
        {
            currentLOD = lod;
            for(int i = 0; i < runtimeGameObjectLOD.Length; i++)
            {
                runtimeGameObjectLOD[i].SetActive(i == currentLOD && isActive);
            }
        }
        
        public void SetLOD(Vector3 playerPosition)
        {
            for(int i = 0; i < viewDistance.Length; i++)
            {
                if(Vector3.Distance(currentGameObject.transform.position, playerPosition) < viewDistance[i])
                {
                    SetLOD(i);
                    return;
                }
            }

            SetActive(false);
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

    private void Awake()
    {
        objectPool = new();
        foreach (var placeableObject in objectTable.placeableObjects)
        {
            objectPool.Add(placeableObject.objectName, new ());
        }
    }

    PlaceableObject GetPlaceableObject(string objectName, int lod = 0)
    {
        Assert.IsTrue(objectPool.ContainsKey(objectName),
            "Placeable objects does not contain objectName: " + objectName);

        foreach (var obj in objectPool[objectName])
        {
            if (obj.isActive == false)
            {
                obj.currentLOD = lod;
                obj.SetActive(true);
                return obj;
            }
        }

        // If there is no available object in the pool, instantiate a new one
        PlaceableObject newObject = null;
        foreach (var placeableObject in objectTable.placeableObjects)
        {
            if (placeableObject.objectName == objectName)
            {
                Assert.IsFalse(placeableObject.gameObjectLOD.Length <= lod,
                    "Object " + objectName + " does not have LOD " + lod);
                
                newObject = new PlaceableObject(objectName, placeableObject.gameObjectLOD, placeableObject.viewDistance);
            }
        }
        
        Assert.IsFalse(newObject == null, "Object " + objectName + " not found in objectTable");

        return newObject;
    }

    void DisableObject(PlaceableObject obj)
    {
        obj.SetActive(false);
    }


    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation, string objectName)
    {
        PlaceableObject newPlaceableObject = GetPlaceableObject(objectName);
        newPlaceableObject.SetTransform(worldPosition, objectSize, objectRotation);
        currentObjects.Add(newPlaceableObject);
    }

    public void UpdatePlacer(Vector3 playerPosition)
    {
        foreach (var obj in currentObjects)
        {
            obj.SetLOD(playerPosition);
        }
    }
}