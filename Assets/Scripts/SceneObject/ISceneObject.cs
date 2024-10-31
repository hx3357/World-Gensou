using UnityEngine;

public interface ISceneObject
{
    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation);
    
    public void RemoveObject(Vector3 worldPosition);
}