using System;
using UnityEngine;
using System.Collections.Generic;

public class GrassInteractableDetector : MonoBehaviour
{
    private const int MAX_INTERACTABLE_COUNT = 10;
    public List<GrassInteractable> curInteractables;
    
    public List<GrassInteractable> GetInteractableObjects()
    {
        return curInteractables;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Grass Interactable"))
        {
            if (curInteractables.Count >= MAX_INTERACTABLE_COUNT)
            {
                return;
            }
            curInteractables.Add(other.gameObject.GetComponent<GrassInteractable>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Grass Interactable"))
        {
            curInteractables.Remove(other.gameObject.GetComponent<GrassInteractable>());
        }
    }
}
