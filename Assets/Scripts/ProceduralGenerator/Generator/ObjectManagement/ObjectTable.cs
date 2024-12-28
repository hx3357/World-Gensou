using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectTable", menuName = "ObjectTable" )]
public class ObjectTable : ScriptableObject
{
    [Serializable]
    public struct PlaceableObjectData
    {
        public string objectName;
        public GameObject[] gameObjectLOD;
        public float[] viewDistance;
        public int maxCount;
    }
    
    public PlaceableObjectData[] placeableObjects;
}

