using System;
using UnityEngine;

public static class PseudoRandomUtility
{
    public static int Seed { get; set; }

    private static int _seed = 114514;
    private static System.Random prng;

    public static float GetSrand(Vector2Int pos, float min, float max)
    {
        var posHash = pos.GetHashCode();
        prng = new System.Random(posHash + _seed);
        return (float)prng.NextDouble() * (max - min) + min;
    }

    public static float GetSrand(Vector2Int pos)
    {
        var posHash = pos.GetHashCode();
        prng = new System.Random(posHash + _seed);
        return (float)prng.NextDouble();
    }

    public static float GetSrand(Vector3Int pos, float min, float max)
    {
        var posHash = pos.GetHashCode();
        prng = new System.Random(posHash + _seed);
        return (float)prng.NextDouble() * (max - min) + min;
    }

    public static float GetSrand(Vector3Int pos)
    {
        var posHash = pos.GetHashCode();
        prng = new System.Random(posHash + _seed);
        return (float)prng.NextDouble();
    }

    public static float GetPerlin(Vector2Int pos)
    {
        prng = new System.Random(_seed);
        return Mathf.PerlinNoise(prng.Next(-10000, 10000) * 0.1f + pos.x, prng.Next(-10000, 10000) * 0.1f + pos.y);
    }
}