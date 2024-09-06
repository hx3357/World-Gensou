using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PerlinNoiseScalerFieldGenerator2D : IScalerFieldGenerator
{
    private Vector4[] dotField;
    private Vector3Int size;
    private int octaves;
    private float scale,persistance, lacunarity;
    private int seed;
    private Vector3 offset;
    private float maxHeight;
    private AnimationCurve heightMapping;
    private float heightOffset;
    private float heightScale;
    private Vector2 randomOffset;
    private Vector3 cellsize;
    private bool isEmptyFlag = false;
    
    public PerlinNoiseScalerFieldGenerator2D(int m_octaves,float m_scale,float m_persistance,
        float m_lacunarity,int m_seed,float maxHeight,AnimationCurve heightMapping,float heightOffset,float heightScale)
    {
        SetParameters(m_octaves,m_scale,m_persistance,m_lacunarity,m_seed,offset, maxHeight, heightMapping,
            heightOffset,heightScale);
    }
    
    void GenerateRandomOffset()
    {
        System.Random prng = new System.Random(seed);
        randomOffset = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000));
    }

    void SetParameters(int m_octaves, float m_scale,float m_persistance, float m_lacunarity,int m_seed,
        Vector2 m_offset,float m_maxHeight,AnimationCurve m_heightMapping,float m_heightOffset,float m_heightScale)
    {
        octaves = m_octaves;
        scale = m_scale;
        persistance = m_persistance;
        lacunarity = m_lacunarity;
        seed = m_seed;
        offset = m_offset;
        maxHeight = m_maxHeight;
        heightMapping = m_heightMapping;
        heightOffset =m_heightOffset;
        heightScale = m_heightScale;
        GenerateRandomOffset();
    }
    
    /// <summary>
    /// Generate a 2D noise map based on offset corresponding to world position
    /// </summary>
    /// <returns></returns>
    float[,] GenerateNoiseMap()
    {
        float[,] noiseMap = new float[size.x,size.z];
        float maxPossibleHeight = float.MinValue;
        float minPossibleHeight = float.MaxValue;
        for(int x=0;x<size.x;x++)
        for (int z = 0; z < size.z; z++)
        {
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;
            float perlinValue = 0;
            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (offset.x + x*cellsize.x) / scale * frequency;
                float sampleY = (offset.z + z*cellsize.z) / scale * frequency;
                perlinValue = Mathf.PerlinNoise(sampleX+randomOffset.x, sampleY+randomOffset.y)*2-1;
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
        return noiseMap;
    }
    
    public void SetParameters(params object[] parameters)
    {
        
    }

    public ScalerFieldRequestData StartGenerateDotField(Vector3 origin, Vector3Int dotfieldSize, Vector3 m_cellsize)
    {
        offset = origin;
        size = dotfieldSize;
        cellsize = m_cellsize;
        float[,] noiseMap = GenerateNoiseMap();
        dotField = new Vector4[size.x* size.y*size.z];
        bool isAirFlag = origin.y>maxHeight;
        bool isUnderGroundFlag = origin.y<0;
        isEmptyFlag = isAirFlag || isUnderGroundFlag;
        if(!isEmptyFlag)
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        Vector3 pos = new Vector3(x*cellsize.x,y*cellsize.y,z*cellsize.z);
                        dotField[ProcedualGeneratorUtility.GetBufferIndex(x, y, z, size)] = pos;
                        float noiseValue = noiseMap[x, z];
                        int dotScalar =  pos.y+origin.y>maxHeight*heightMapping.Evaluate(heightScale* noiseValue+heightOffset)  ? 1 : 0;
                        dotField[ProcedualGeneratorUtility.GetBufferIndex(x, y, z, size)].w = dotScalar;
                    }
                }
            }

        return new ScalerFieldRequestData();
    }
    
    public (bool,Vector4[],bool) GetState(ref ScalerFieldRequestData scalerFieldRequestData)
    {
        return (true,GetDotField(scalerFieldRequestData, out bool isEmpty),isEmpty);
    }
    
    private Vector4[] GetDotField(ScalerFieldRequestData scalerFieldRequestData, out bool isEmptyFlag)
    {
        isEmptyFlag = this.isEmptyFlag;
        return dotField;
    }
    
    public void Release(ScalerFieldRequestData scalerFieldRequestData)
    {
        dotField = null;
    }
}