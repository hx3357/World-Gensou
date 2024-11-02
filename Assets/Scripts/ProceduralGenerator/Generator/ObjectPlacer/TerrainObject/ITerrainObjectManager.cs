using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITerrainObjectManager
{
    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation);

    public void UpdateObjects(Vector3 playerPosition);
    
    public static void UpdateObjectsFunc(in ICollection<Vector3> visibleSet,in ICollection<Vector3> invisibleSet,
        Vector3 playerPosition,float viewDistance,float existDistance)
    {
        List<Vector3> visibleSetRemoveList = new List<Vector3>();
        List<Vector3> invisibleSetRemoveList = new List<Vector3>();
        
        foreach (var visiblePos in visibleSet)
        {
            float distance = Vector3.Distance(visiblePos, playerPosition);
            if (distance > viewDistance)
            {
                invisibleSet.Add(visiblePos);
                visibleSetRemoveList.Add(visiblePos);
            }else if (distance > existDistance)
            {
                visibleSetRemoveList.Add(visiblePos);
            }
        }

        foreach (var invisiblePos in invisibleSet)
        {
            float distance = Vector3.Distance(invisiblePos, playerPosition);
            if (distance < viewDistance)
            {
                visibleSet.Add(invisiblePos);
                invisibleSetRemoveList.Add(invisiblePos);
            }else if (distance > existDistance)
            {
                invisibleSetRemoveList.Add(invisiblePos);
            }
        }
        
        for(int i = 0; i < visibleSetRemoveList.Count; i++)
        {
            visibleSet.Remove(visibleSetRemoveList[i]);
        }
        
        for(int i = 0; i < invisibleSetRemoveList.Count; i++)
        {
            invisibleSet.Remove(invisibleSetRemoveList[i]);
        }
    }
}