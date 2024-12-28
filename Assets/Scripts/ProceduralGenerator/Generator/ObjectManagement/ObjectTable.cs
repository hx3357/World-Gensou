using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectTable", menuName = "ObjectTable" )]
public class ObjectTable : ScriptableObject
{
    [Serializable]
    public struct PlaceableObjectData
    {
        public string objectName; // Main key
        public GameObject gameObjectPrefab;
        public float viewDistance;
        public int maxCount;
    }

    public PlaceableObjectData[] placeableObjects;

    public PlaceableObjectData FindPlaceableObjectDataByName(string objName)
    {
        foreach (var placeableObjectData in placeableObjects)
        {
            if (objName == placeableObjectData.objectName)
            {
                return placeableObjectData;
            }
        }

        return default;
    }

    public PlaceableObjectData FindPlaceableObjectDataByInstance(ObjectPlacer.PlaceableObject obj)
    {
        foreach (var placeableObjectData in placeableObjects)
        {
            if (obj.objectName == placeableObjectData.objectName)
            {
                return placeableObjectData;
            }
        }

        return default;
    }
}

