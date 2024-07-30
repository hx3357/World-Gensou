using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUScalerFieldGenerator : IScalerFieldGenerator
{
    private Vector3 origin;
    private Vector3Int size;
    private Vector3 cellsize;
    
    protected object[] parameters;
    private ComputeShader cs;
    
    private ComputeBuffer outputPointBuffer;
    
    protected GPUScalerFieldGenerator(ComputeShader m_cs,params object[] m_parameters)
    {
        cs = m_cs;
        parameters = m_parameters;
    }
    
    public object[] GetParameters()
    {
        return parameters;
    }
    
    public void SetParameters(params object[] m_parameters)
    {
        parameters = m_parameters;
    }

    void InitBuffer()
    {
        outputPointBuffer = new ComputeBuffer(size.x*size.y*size.z, sizeof(float)*4);
    }
    
    void ReleaseBuffer()
    {
        outputPointBuffer.Release();
    }
    
    void RunNoiseComputeShader()
    {
        int kernel = cs.FindKernel("CSMain");
        cs.SetInts("size", size.x, size.y, size.z);
        cs.SetVector("origin",  origin);
        cs.SetVector("cellSize", cellsize);
        cs.SetBuffer(kernel, "outputPoints", outputPointBuffer);
        cs.Dispatch(kernel, size.x/8, size.y/8, size.z/8);
    }

    public virtual Vector4[] GenerateDotField( Vector3 m_origin,Vector3Int m_size, Vector3 m_cellsize)
    {
        origin = m_origin;
        size = m_size;
        cellsize = m_cellsize;
        return null;
    }

    public void ShowDotFieldGizmos()
    {
        
    }
}
