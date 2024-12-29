using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LODConfig", menuName = "PCG")]
public class LODConfig : ScriptableObject
{
    public struct LODKVPair
    {
        public float distance;
        public int resoulution;
    }

    public LODKVPair[] lodConfig;
    public int fallbackResolution;

    public Dictionary<float, int> GetLODConfigTable()
    {
        Dictionary<float, int> config = new();
        foreach (var kvpair in lodConfig) config.Add(kvpair.distance, kvpair.resoulution);

        return config;
    }
}