
using System.Transactions;
using UnityEngine;

public interface IObjectPlacer
{
    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation, string objectName);
    
    public void UpdatePlacer(Vector3 playerPosition);
}
