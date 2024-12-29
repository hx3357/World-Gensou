using UnityEngine;

public class TerrainObject
{
    public Vector3 worldPosition;
    public Vector3 objectSize;
    public Vector3 objectRotation;

    public TerrainObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation)
    {
        this.worldPosition = worldPosition;
        this.objectSize = objectSize;
        this.objectRotation = objectRotation;
    }
}