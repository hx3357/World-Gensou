using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;
    public static int SizeOf => sizeof(float)*3*3;
}

