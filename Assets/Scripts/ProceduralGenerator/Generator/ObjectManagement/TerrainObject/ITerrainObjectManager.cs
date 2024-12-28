using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public interface ITerrainObjectManager
{
    public void PlaceObject(Vector3 worldPosition, Vector3 objectSize, Vector3 objectRotation);

    public void UpdateObjects(Vector3 playerPosition);
    
    public static void UpdateObjectsFunc(in ICollection<Vector3> visibleSet,
        Vector3 playerPosition,float viewDistance)
    {
        List<Vector3> visibleSetRemoveList = new List<Vector3>();

        Parallel.ForEach(visibleSet, visiblePos =>
        {
            float distance = Vector3.Distance(visiblePos, playerPosition);
            if (distance > viewDistance)
            {
                lock (visibleSetRemoveList)
                {
                    visibleSetRemoveList.Add(visiblePos);
                }
            }
        });
        
        for(int i = 0; i < visibleSetRemoveList.Count; i++)
        {
            visibleSet.Remove(visibleSetRemoveList[i]);
        }
    }
}