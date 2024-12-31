using UnityEngine;
using System.Collections.Generic;

public class GrassInteractable : MonoBehaviour
{
    public Vector3 position => transform.position;
    public float radius = 1;

    public static (Vector4[] positions, float[] radiusList) ToArray(GrassInteractable[] interactables)
    {
        Vector4[] positions = new Vector4[interactables.Length];
        float[] radiusList = new float[interactables.Length];
        for (int i = 0; i < interactables.Length; i++)
        {
            positions[i] = interactables[i].position;
            radiusList[i] = interactables[i].radius;
        }

        return (positions, radiusList);
    }
}
