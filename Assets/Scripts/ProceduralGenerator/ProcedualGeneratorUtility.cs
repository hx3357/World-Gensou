using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProcedualGeneratorUtility
{
    public static int GetBufferIndex(int x, int y, int z, Vector3Int size)
    {
        return x + y * size.x + z * size.x * size.y;
    }

    public static void ShowDotFieldGizmo(Vector3 origin, Vector3Int size, Vector4[] dotField)
    {
        for (var x = 0; x < size.x; x++)
        for (var y = 0; y < size.y; y++)
        for (var z = 0; z < size.z; z++)
        {
            Gizmos.color = Color.Lerp(Color.black, Color.white, dotField[GetBufferIndex(x, y, z, size)].w);
            Gizmos.DrawCube((Vector3)dotField[GetBufferIndex(x, y, z, size)] + origin, Vector3.one * 0.1f);
        }
    }

    public static bool isInSurroundBox(Vector3Int chunkCoord, int[] surroundBox)
    {
        return chunkCoord.x <= surroundBox[0] && chunkCoord.x >= surroundBox[1] &&
               chunkCoord.y <= surroundBox[2] && chunkCoord.y >= surroundBox[3] &&
               chunkCoord.z <= surroundBox[4] && chunkCoord.z >= surroundBox[5];
    }
}