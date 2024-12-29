using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

[StructLayout(LayoutKind.Explicit)]
public struct Dot
{
    [FieldOffset(0)] public float w;
    [FieldOffset(4)] public byte r;
    [FieldOffset(5)] public byte g;
    [FieldOffset(6)] public byte b;
    [FieldOffset(7)] private float padding;
    [FieldOffset(11)] private float padding2;
    [FieldOffset(15)] private byte padding3;

    public void SetValue(float value, byte r, byte g, byte b)
    {
        w = value;
        this.r = r;
        this.g = g;
        this.b = b;
    }

    public static int GetSize()
    {
        return 16;
    }
}