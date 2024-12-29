using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;

public class PerlinNoise3D
{
    private static int seed;
    private static Random prng;
    private static Vector2 randomOffset;

    public PerlinNoise3D()
    {
        prng = new Random(seed);
    }

    public void SetRandomSeed(int m_seed)
    {
        seed = m_seed;
        randomOffset = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000));
    }

    public float Get3DPerlin(Vector3 position)
    {
        var x = position.x;
        var y = position.y + 1;
        var z = position.z + 2;
        var xy = _perlin3DFixed(x, y);
        var xz = _perlin3DFixed(x, z);
        var yz = _perlin3DFixed(y, z);
        var yx = _perlin3DFixed(y, x);
        var zx = _perlin3DFixed(z, x);
        var zy = _perlin3DFixed(z, y);
        return xy * xz * yz * yx * zx * zy;
    }

    private float _perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a + randomOffset.x, b + randomOffset.y));
    }
}