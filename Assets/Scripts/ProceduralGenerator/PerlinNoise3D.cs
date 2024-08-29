using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;

public class PerlinNoise3D 
{
    static int seed;
    static Random prng;
    static Vector2 randomOffset;
    
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
       float x = position.x;
       float y = position.y + 1;
       float z = position.z + 2;
       float xy = _perlin3DFixed(x, y);
       float xz = _perlin3DFixed(x, z);
       float yz = _perlin3DFixed(y, z);
       float yx = _perlin3DFixed(y, x);
       float zx = _perlin3DFixed(z, x);
       float zy = _perlin3DFixed(z, y);
       return xy * xz * yz * yx * zx * zy;
    }
    
    private float _perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a+randomOffset.x, b+randomOffset.y));
    }
}
