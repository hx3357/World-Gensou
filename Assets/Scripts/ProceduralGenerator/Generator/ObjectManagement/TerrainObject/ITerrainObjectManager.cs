using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public interface ITerrainObjectManager
{
    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation);

    public void UpdateObjects(Vector3 playerPosition);

    
}