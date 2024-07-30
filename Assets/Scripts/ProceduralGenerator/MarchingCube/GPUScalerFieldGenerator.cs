using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUScalerFieldGenerator : IScalerFieldGenerator
{
    private Vector3 origin;
    private Vector3Int dotFieldCount;
    private Vector3 cellsize;
    
    protected object[] parameters;
    private ComputeShader cs;
    
    private ComputeBuffer outputPointBuffer;
    
    protected GPUScalerFieldGenerator(ComputeShader m_cs,params object[] m_parameters)
    {
        cs = m_cs;
        parameters = m_parameters;
    }

    void InitBuffer()
    {
        outputPointBuffer = new ComputeBuffer(dotFieldCount.x*dotFieldCount.y*dotFieldCount.z, sizeof(float)*4);
    }
    
    void ReleaseBuffer()
    {
        outputPointBuffer.Release();
    }
    
    void RunNoiseComputeShader()
    {
        int kernel = cs.FindKernel("CSMain");
        cs.SetInts("dotFieldCount", dotFieldCount.x, dotFieldCount.y, dotFieldCount.z);
        cs.SetVector("origin",  origin);
        cs.SetVector("cellSize", cellsize);
        cs.SetBuffer(kernel, "outputPoints", outputPointBuffer);
        cs.Dispatch(kernel, dotFieldCount.x/8, dotFieldCount.y/8, dotFieldCount.z/8);
    }

    public virtual Vector4[] GenerateDotField( Vector3 m_origin,Vector3Int m_size, Vector3 m_cellsize)
    {
        origin = m_origin;
        dotFieldCount = m_size;
        cellsize = m_cellsize;
        InitBuffer();
        RunNoiseComputeShader();
        Vector4[] dotField = new Vector4[dotFieldCount.x*dotFieldCount.y*dotFieldCount.z];
        outputPointBuffer.GetData(dotField);
        ReleaseBuffer();
        return null;
    }
}
