using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseScalerFieldGenerator3D : IScalerFieldGenerator
{
    private readonly int seed;
    private readonly float scale;
    private readonly float isoLevel;
    
    private bool isEmptyFlag = false;
    private Dot[] dotField;

    private PerlinNoise3D prng;
    
    public PerlinNoiseScalerFieldGenerator3D(int m_seed,float m_scale,float m_isoLevel)
    {
        seed = m_seed;
        prng = new PerlinNoise3D();
        prng.SetRandomSeed(seed);
        
        scale = m_scale;
        isoLevel = m_isoLevel;
    }
    
    public void SetParameters(params object[] parameters)
    {
        
    }
    
    public ScalerFieldRequestData StartGenerateDotField(Vector3 origin, Vector3Int dotfieldSize, Vector3 m_cellsize,
        object[] parameters = null)
    {
        bool isConcreteFlag = true,isAirFlag = true;
        dotField = new Dot[dotfieldSize.x*dotfieldSize.y*dotfieldSize.z];
        for(int x=0;x<dotfieldSize.x;x++)
            for(int y=0;y<dotfieldSize.y;y++)
                for(int z=0;z<dotfieldSize.z;z++)
                {
                    Vector3 position = new Vector3(x*m_cellsize.x,y*m_cellsize.y,z*m_cellsize.z);
                    float value = prng.Get3DPerlin((origin+position)/scale);
                    dotField[x + y * dotfieldSize.x + z * dotfieldSize.x * dotfieldSize.y] = new Dot
                    {
                        w = value,
                        r = 255,
                        g = 255,
                        b = 255,
                    };
                    if(value>isoLevel)
                        isConcreteFlag = false;
                    else
                        isAirFlag = false;
                }
        isEmptyFlag = isConcreteFlag || isAirFlag;
        return new ScalerFieldRequestData();
    }
    
    public (bool,Dot[],bool) GetState(ref ScalerFieldRequestData scalerFieldRequestData,bool isNotGetDotfield = false)
    {
        return (true,GetDotField(scalerFieldRequestData),this.isEmptyFlag);
    }
    
    private Dot[] GetDotField(ScalerFieldRequestData scalerFieldRequestData)
    {
        return dotField;
    }
    
    public void Release(ScalerFieldRequestData scalerFieldRequestData, bool isReleaseDotfieldBuffer)
    {
        dotField = null;
    }
}
