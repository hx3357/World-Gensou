using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static int GetBufferIndex(int x, int y, int z, Vector3Int size)
    {
        return x + y * size.x + z * size.x * size.y;
    }
    
    public static void ShowDotFieldGizmos(Vector3Int size, Vector4[] dotField)
    {
        for(int x=0;x<size.x;x++)
        for(int y=0;y<size.y;y++)
        for(int z=0;z<size.z;z++)
        {
            Gizmos.color = Color.Lerp(Color.black, Color.white, dotField[Utility.GetBufferIndex(x, y, z, size)].w);
            Gizmos.DrawCube(dotField[Utility.GetBufferIndex(x, y, z, size)], Vector3.one * 0.1f);
        }
    }
}
