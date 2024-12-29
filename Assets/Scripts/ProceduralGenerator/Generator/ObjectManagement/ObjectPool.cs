using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlaceableObjectPool
{
    public List<ObjectPlacer.PlaceableObject> activeObjects { get; private set; } = new();

    // Key: objectName, Value: inactive objects
    private Dictionary<string, Queue<ObjectPlacer.PlaceableObject>> inactiveObjectMap = new();
    private ObjectTable _placeableObjectTable;
    private Transform _parentTransform;

    public PlaceableObjectPool(ObjectTable placeableObjectTable, Transform parentTransform)
    {
        _placeableObjectTable = placeableObjectTable;
        _parentTransform = parentTransform;
        foreach (var placeableObject in placeableObjectTable.placeableObjects)
            inactiveObjectMap.Add(placeableObject.objectName, new Queue<ObjectPlacer.PlaceableObject>());
    }

    public ObjectPlacer.PlaceableObject GetObject(string objectName)
    {
        Assert.IsTrue(inactiveObjectMap.ContainsKey(objectName),
            "Placeable objects does not contain objectName: " + objectName);

        // Use the object in the pool
        if (GetObjCount(objectName) > 0)
        {
            var obj = inactiveObjectMap[objectName].Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // If there is no available object in the pool, instantiate a new one
        ObjectPlacer.PlaceableObject newObj = null;
        foreach (var obj in _placeableObjectTable.placeableObjects)
            if (obj.objectName == objectName)
            {
                newObj = new ObjectPlacer.PlaceableObject(objectName, obj.gameObjectPrefab,
                    obj.viewDistance, _parentTransform);
                break;
            }

        Assert.IsNotNull(newObj,
            "Placeable object table does not contain objectName: " + objectName);
        activeObjects.Add(newObj);
        return newObj;
    }

    public void DisableObject(ObjectPlacer.PlaceableObject obj)
    {
        activeObjects.Remove(obj);

        if (GetObjCount(obj.objectName) >=
            _placeableObjectTable.FindPlaceableObjectDataByInstance(obj).maxCount)
        {
            obj.Destory();
            return;
        }

        obj.SetActive(false);
        inactiveObjectMap[obj.objectName].Enqueue(obj);
    }

    public int GetObjCount(string objName)
    {
        return inactiveObjectMap[objName].Count;
    }
}