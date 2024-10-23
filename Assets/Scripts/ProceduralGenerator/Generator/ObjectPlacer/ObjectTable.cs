using System;
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
    }
    
    public PlaceableObjectData[] placeableObjects;
}

