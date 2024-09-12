using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public struct ScalerFieldRequestData
{
    public AsyncGPUReadbackRequest[] requests;
    public ComputeBuffer[] buffers;
    public List<object> extraParameters;
    public bool[] isDoneFlags;
}

public class GPUScalerFieldGenerator : IScalerFieldGenerator
{
    private Vector3 origin;
    private Vector3Int dotFieldSize;
    private Vector3 cellsize;
    
    protected ComputeShader cs;
    
    protected List<ComputeBuffer> buffers = new List<ComputeBuffer>();
    
    private static readonly int DotFieldSize = Shader.PropertyToID("dotFieldSize");
    private static readonly int Origin = Shader.PropertyToID("origin");
    private static readonly int CellSize = Shader.PropertyToID("cellSize");
    private static readonly int OutputPoints = Shader.PropertyToID("outputPoints");
    
    private AsyncGPUReadbackRequest request;
    protected List<AsyncGPUReadbackRequest> requests = new List<AsyncGPUReadbackRequest>();
    
    protected object[] parameters;

    protected GPUScalerFieldGenerator(ComputeShader m_cs)
    {
        cs = m_cs;
    }

    protected virtual void InitBuffer()
    {
        ComputeBuffer outputPointBuffer = new ComputeBuffer(dotFieldSize.x*dotFieldSize.y*dotFieldSize.z, sizeof(float)*4);
        buffers.Add(outputPointBuffer);
    }
    
    public virtual void Release(ScalerFieldRequestData scalerFieldRequestData)
    {
        foreach (var i in scalerFieldRequestData.buffers)
        {
            i.Release();
        }
    }

    protected virtual void GenerateRequest(ScalerFieldRequestData scalerFieldRequestData)
    {
        request = AsyncGPUReadback.Request(scalerFieldRequestData.buffers[0],
            dotFieldSize.x*dotFieldSize.y*dotFieldSize.z*sizeof(float)*4,0);
        requests.Add(request);
    }
    
    void RunNoiseComputeShader( ScalerFieldRequestData scalerFieldRequestData)
    {
        int kernel = 0;
        cs.SetInts(DotFieldSize, dotFieldSize.x, dotFieldSize.y, dotFieldSize.z);
        cs.SetVector(Origin,  origin);
        cs.SetVector(CellSize, cellsize);
        cs.SetBuffer(kernel, OutputPoints, scalerFieldRequestData.buffers[0]);
        SetComputeShaderParameters(cs,scalerFieldRequestData);
        cs.Dispatch(kernel, Mathf.CeilToInt(dotFieldSize.x / 8.0f), 
            Mathf.CeilToInt(dotFieldSize.y / 8.0f), 
            Mathf.CeilToInt(dotFieldSize.z / 4.0f));
    }
    
    protected virtual void SetComputeShaderParameters(ComputeShader m_cs,ScalerFieldRequestData scalerFieldRequestData){ }

    protected virtual bool GetEmptyState(ScalerFieldRequestData scalerFieldRequestData)
    {
        return false;
    }

    public virtual void SetParameters(object[] m_parameters)
    {
        parameters = m_parameters;
    }

    public virtual ScalerFieldRequestData StartGenerateDotField(Vector3 m_origin,Vector3Int m_dotfieldSize, Vector3 m_cellsize)
    {
        origin = m_origin;
        dotFieldSize = m_dotfieldSize;
        cellsize = m_cellsize;
        ScalerFieldRequestData scalerFieldRequestData = new ScalerFieldRequestData();
        buffers.Clear();
        requests.Clear();
        
        InitBuffer();
        scalerFieldRequestData.buffers = buffers.ToArray();
        
        RunNoiseComputeShader(scalerFieldRequestData);
        
        GenerateRequest(scalerFieldRequestData);
        scalerFieldRequestData.requests = requests.ToArray();
        
        return scalerFieldRequestData;
    }
    
    public virtual (bool,Vector4[],bool) GetState(ref ScalerFieldRequestData scalerFieldRequestData)
    {
        if (scalerFieldRequestData.requests[0].done)
            return (true,GetDotField(scalerFieldRequestData, out bool isEmpty),isEmpty);
        else
            return (false,null,false);
    }
    
    private Vector4[] GetDotField(ScalerFieldRequestData scalerFieldRequestData, out bool isEmpty)
    {
        Vector4[] dotField = new Vector4[dotFieldSize.x*dotFieldSize.y*dotFieldSize.z];
        scalerFieldRequestData.requests[0].GetData<Vector4>().CopyTo(dotField);
        isEmpty = GetEmptyState(scalerFieldRequestData);
        return dotField;
    }
    
    
}
