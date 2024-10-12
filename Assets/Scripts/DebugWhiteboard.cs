using System;
using UnityEngine;

public class DebugWhiteboard: MonoBehaviour
{
    private SurroundBox surroundBox;
    private void Start()
    {
        surroundBox = new SurroundBox(new Vector3(0,0,0),
            new Vector3(200,200,200) * 2 + 2*Chunk.GetWorldSize());
    }

    private void OnDrawGizmos()
    {
       // surroundBox?.DrawSurroundBoxGizmo();
    }
}