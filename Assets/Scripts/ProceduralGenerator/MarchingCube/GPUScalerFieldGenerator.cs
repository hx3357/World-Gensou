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
    private static readonly int DotFieldCount = Shader.PropertyToID("dotFieldCount");
    private static readonly int Origin = Shader.PropertyToID("origin");
    private static readonly int CellSize = Shader.PropertyToID("cellSize");
    private static readonly int OutputPoints = Shader.PropertyToID("outputPoints");

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
        int kernel = 0;
        cs.SetInts(DotFieldCount, dotFieldCount.x, dotFieldCount.y, dotFieldCount.z);
        cs.SetVector(Origin,  origin);
        cs.SetVector(CellSize, cellsize);
        cs.SetBuffer(kernel, OutputPoints, outputPointBuffer);
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
