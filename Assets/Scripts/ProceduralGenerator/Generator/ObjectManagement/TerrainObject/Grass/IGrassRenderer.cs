using UnityEngine;

public interface IGrassRenderer
{
    public void DrawGrass(Vector3[] positions, Vector3 playerPosition, GrassInteractable[] interactables);
}