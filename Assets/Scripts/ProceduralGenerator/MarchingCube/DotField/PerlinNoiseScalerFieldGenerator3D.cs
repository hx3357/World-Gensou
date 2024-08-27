using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseScalerFieldGenerator3D : IScalerFieldGenerator
{
    private readonly int seed;
    private readonly float scale;
    private readonly float isoLevel;
    
    private bool isEmptyFlag = false;
    private Vector4[] dotField;
    
    public PerlinNoiseScalerFieldGenerator3D(int m_seed,float m_scale,float m_isoLevel)
    {
        seed = m_seed;
        PerlinNoise3D.SetRandomSeed(seed);
        
        scale = m_scale;
        isoLevel = m_isoLevel;
    }
    
    public ScalerFieldRequestData StartGenerateDotField(Vector3 origin, Vector3Int dotfieldSize, Vector3 m_cellsize)
    {
        bool isConcreteFlag = true,isAirFlag = true;
        dotField = new Vector4[dotfieldSize.x*dotfieldSize.y*dotfieldSize.z];
        for(int x=0;x<dotfieldSize.x;x++)
            for(int y=0;y<dotfieldSize.y;y++)
                for(int z=0;z<dotfieldSize.z;z++)
                {
                    Vector3 position = new Vector3(x*m_cellsize.x,y*m_cellsize.y,z*m_cellsize.z);
                    float value = PerlinNoise3D.Get3DPerlin((origin+position)/scale);
                    dotField[x + y * dotfieldSize.x + z * dotfieldSize.x * dotfieldSize.y] = new Vector4(position.x,position.y,position.z,value);
                    if(value>isoLevel)
                        isConcreteFlag = false;
                    else
                        isAirFlag = false;
                }
        isEmptyFlag = isConcreteFlag || isAirFlag;
        return new ScalerFieldRequestData();
    }
    
    public bool GetState(ScalerFieldRequestData scalerFieldRequestData)
    {
        return true;
    }
    
    public Vector4[] GetDotField(ScalerFieldRequestData scalerFieldRequestData, out bool isEmptyFlag)
    {
        isEmptyFlag = this.isEmptyFlag;
        return dotField;
    }
}
