using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PerlinNoiseScalerFieldGenerator_2D : CPUNoiseScalerFieldGenerator
{
    private int octaves;
    private float scale,persistance, lacunarity;
    private int seed;
    private Vector2 offset;
    private float maxHeight;
    private AnimationCurve heightMapping;
    
    public PerlinNoiseScalerFieldGenerator_2D(int m_octaves,float m_scale,float m_persistance,
        float m_lacunarity,int m_seed,float maxHeight,AnimationCurve heightMapping,params object[] parameters): base(parameters)
    {
        SetParameters(m_octaves,m_scale,m_persistance,m_lacunarity,m_seed,offset, maxHeight, heightMapping);
    }

    public void SetParameters(int m_octaves, float m_scale,float m_persistance, float m_lacunarity,int m_seed,
        Vector2 m_offset,float m_maxHeight,AnimationCurve m_heightMapping)
    {
        octaves = m_octaves;
        scale = m_scale;
        persistance = m_persistance;
        lacunarity = m_lacunarity;
        seed = m_seed;
        offset = m_offset;
        maxHeight = m_maxHeight;
        heightMapping = m_heightMapping;
    }
    
    /// <summary>
    /// Generate a 2D noise map based on offset corresponding to world position
    /// </summary>
    /// <returns></returns>
    float[,] GenerateNoiseMap()
    {
        float[,] noiseMap = new float[size.x,size.z];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000)+offset.x;
            float offsetY = prng.Next(-100000, 100000)+offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        float maxPossibleHeight = float.MinValue;
        float minPossibleHeight = float.MaxValue;
        for(int x=0;x<size.x;x++)
        for (int z = 0; z < size.z; z++)
        {
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;
            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (x-size.x/2.0f) / scale * frequency + octaveOffsets[i].x;
                float sampleY = (z-size.z/2.0f) / scale * frequency + octaveOffsets[i].y;
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY)*2-1;
                noiseHeight += perlinValue * amplitude;
                amplitude *= persistance;
                frequency *= lacunarity;
            }
            if(noiseHeight>maxPossibleHeight)
                maxPossibleHeight = noiseHeight;
            if(noiseHeight<minPossibleHeight)
                minPossibleHeight = noiseHeight;
            noiseMap[x, z] = noiseHeight;
        }
        for(int x=0;x<size.x;x++)
        for(int z=0;z<size.z;z++)
        {
            noiseMap[x,z] = Mathf.InverseLerp(minPossibleHeight, maxPossibleHeight, noiseMap[x,z]);
        }
        return noiseMap;
    }

    public override Vector4[] GenerateDotField(Vector3 origin, Vector3Int m_size, Vector3 cellsize)
    {
        origin = offset;
        size = m_size;
        float[,] noiseMap = GenerateNoiseMap();
        dotField = new Vector4[size.x* size.y*size.z];
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3 pos = new Vector3(origin.x+x*cellsize.x,origin.y+y*cellsize.y,origin.z+z*cellsize.z);
                    dotField[ProcedualGeneratorUtility.GetBufferIndex(x, y, z, size)] = pos;
                    dotField[ProcedualGeneratorUtility.GetBufferIndex(x,y,z,size)].w = pos.y>maxHeight*heightMapping.Evaluate(noiseMap[x,z])  ? 1 : 0;
                }
            }
        }
        return dotField;
    }
}