using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public struct ScalerFieldRequestData
{
    public AsyncGPUReadbackRequest[] requests;
    public ComputeBuffer[] buffers;
    public object[] parameters;
    public List<object> resultStates;
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
    
    protected List<AsyncGPUReadbackRequest> requests = new List<AsyncGPUReadbackRequest>();

    protected GPUScalerFieldGenerator(ComputeShader m_cs)
    {
        cs = m_cs;
    }

    protected virtual void InitBuffer()
    {
        ComputeBuffer outputPointBuffer = new ComputeBuffer(dotFieldSize.x*dotFieldSize.y*dotFieldSize.z, 
            Dot.GetSize());
        buffers.Add(outputPointBuffer);
    }
    
    public virtual void Release(ScalerFieldRequestData scalerFieldRequestData, bool isNotReleaseDotfieldBuffer = false)
    {
        foreach (var i in scalerFieldRequestData.buffers)
        {
            if(isNotReleaseDotfieldBuffer&& i == scalerFieldRequestData.buffers[0])
                continue;
                
            i.Release();
        }
    }

    protected virtual void GenerateRequest(ScalerFieldRequestData scalerFieldRequestData)
    {
        AsyncGPUReadbackRequest  request = AsyncGPUReadback.Request(scalerFieldRequestData.buffers[0],
            dotFieldSize.x*dotFieldSize.y*dotFieldSize.z*Dot.GetSize(),0);
        requests.Add(request);
    }
    
    void RunNoiseComputeShader( ScalerFieldRequestData scalerFieldRequestData,object[] parameters)
    {
        int kernel = 0;
        cs.SetInts(DotFieldSize, dotFieldSize.x, dotFieldSize.y, dotFieldSize.z);
        cs.SetVector(Origin,  origin);
        cs.SetVector(CellSize, cellsize);
        cs.SetBuffer(kernel, OutputPoints, scalerFieldRequestData.buffers[0]);
        SetComputeShaderParameters(cs,scalerFieldRequestData,parameters);
        cs.Dispatch(kernel, Mathf.CeilToInt(dotFieldSize.x / 8.0f), 
            Mathf.CeilToInt(dotFieldSize.y / 8.0f), 
            Mathf.CeilToInt(dotFieldSize.z / 8.0f));
    }
    
    protected virtual void SetComputeShaderParameters(ComputeShader m_cs,ScalerFieldRequestData scalerFieldRequestData,object[] parameters){ }

    protected virtual bool GetEmptyState(ScalerFieldRequestData scalerFieldRequestData)
    {
        return false;
    }

    public virtual void SetParameters(object[] m_parameters)
    {
       // parameters = m_parameters;
    }

    public ScalerFieldRequestData StartGenerateDotField(Vector3 m_origin,Vector3Int m_dotfieldSize, Vector3 m_cellsize, object[] m_parameters = null)
    {
        origin = m_origin;
        dotFieldSize = m_dotfieldSize;
        cellsize = m_cellsize;
        ScalerFieldRequestData scalerFieldRequestData = new ScalerFieldRequestData();
        buffers.Clear();
        requests.Clear();
        
        InitBuffer();
        scalerFieldRequestData.buffers = buffers.ToArray();
        
        RunNoiseComputeShader(scalerFieldRequestData,m_parameters);
        
        GenerateRequest(scalerFieldRequestData);
        scalerFieldRequestData.requests = requests.ToArray();
        
        return scalerFieldRequestData;
    }
    
    public virtual (bool,Dot[],bool) GetState(ref ScalerFieldRequestData scalerFieldRequestData,bool isNotGetDotfield = false)
    {
        if (scalerFieldRequestData.requests[0].done)
            return (true, isNotGetDotfield? null: GetDotField(scalerFieldRequestData), GetEmptyState(scalerFieldRequestData));
     
        return (false,null,false);
    }
    
    private Dot[] GetDotField(ScalerFieldRequestData scalerFieldRequestData)
    {
        Dot[] dotField = new Dot[dotFieldSize.x*dotFieldSize.y*dotFieldSize.z];
        scalerFieldRequestData.requests[0].GetData<Dot>().CopyTo(dotField);
        return dotField;
    }
    
}
