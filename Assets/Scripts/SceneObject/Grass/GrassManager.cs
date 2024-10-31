using System.Collections.Generic;
using UnityEngine;

public class GrassManager : MonoSingleton<GrassManager>, ISceneObject
{
    private Dictionary<Vector3,Grass> grassDict = new();
    
    
    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation)
    {
        
    }

    public void RemoveObject(Vector3 worldPosition)
    {
        
    }
}
