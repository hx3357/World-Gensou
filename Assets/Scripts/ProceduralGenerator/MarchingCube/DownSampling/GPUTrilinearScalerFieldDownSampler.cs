using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUTrilinearScalerFieldDownSampler : IScalerFieldDownSampler
{
    private ComputeShader downSamplerShader;
    
    private ComputeBuffer oldDotFieldBuffer;
    private ComputeBuffer newDotFieldBuffer;
    
    private Vector4[] oldDotField;
    private Vector4[] newDotField;
    
    private Vector3 newCellSize;
    
    private Vector3Int oldDotFieldCount;
    private Vector3Int newDotFieldCount;
    
    public GPUTrilinearScalerFieldDownSampler(ComputeShader downSamplerShader)
    {
        this.downSamplerShader = downSamplerShader;
    }
    
    void InitBuffer()
    {
        oldDotFieldBuffer = new ComputeBuffer(oldDotField.Length, sizeof(float)*4);
        oldDotFieldBuffer.SetData(oldDotField);
        
        newDotField = new Vector4[newDotFieldCount.x*newDotFieldCount.y*newDotFieldCount.z];
        newDotFieldBuffer = new ComputeBuffer(newDotFieldCount.x*newDotFieldCount.y*newDotFieldCount.z, sizeof(float)*4);
        newDotFieldBuffer.SetData(newDotField);
    }
    
    void ReleaseBuffer()
    {
        oldDotFieldBuffer.Release();
        newDotFieldBuffer.Release();
    }
    
    void RunDownSampleComputeShader()
    {
        int kernel = downSamplerShader.FindKernel("CSMain");
        downSamplerShader.SetInts("oldDotFieldCount", oldDotFieldCount.x, oldDotFieldCount.y, oldDotFieldCount.z);
        downSamplerShader.SetInts("newDotFieldCount", newDotFieldCount.x, newDotFieldCount.y, newDotFieldCount.z);
        downSamplerShader.SetFloats("newCellSize",newCellSize.x, newCellSize.y, newCellSize.z);
        downSamplerShader.SetBuffer(kernel, "oldDotField", oldDotFieldBuffer);
        downSamplerShader.SetBuffer(kernel, "newDotField", newDotFieldBuffer);
        downSamplerShader.Dispatch(kernel, Mathf.CeilToInt(newDotFieldCount.x / 8.0f), 
            Mathf.CeilToInt(newDotFieldCount.y / 8.0f), 
            Mathf.CeilToInt(newDotFieldCount.z / 8.0f));
    }
    
    public Vector4[] DownSample(Vector4[] dotField, Vector3Int m_dotFieldCount, Vector3 cellSize, float downSampleRate,
        out Vector3Int m_newDotFieldSize ,out Vector3Int m_newChunkSize, out Vector3 m_newCellSize)
    {
        oldDotField = dotField;
        oldDotFieldCount = m_dotFieldCount;
        m_newDotFieldSize = this.newDotFieldCount = new Vector3Int((int)(m_dotFieldCount.x / downSampleRate), 
            (int)(m_dotFieldCount.y / downSampleRate), 
            (int)(m_dotFieldCount.z / downSampleRate));
        m_newChunkSize = new Vector3Int(this.newDotFieldCount.x-1, this.newDotFieldCount.y-1, this.newDotFieldCount.z-1);
        m_newCellSize = this.newCellSize = new Vector3((m_dotFieldCount.x-1) / (float)(m_newChunkSize.x) * cellSize.x, 
            (m_dotFieldCount.y-1) / (float)(m_newChunkSize.y) * cellSize.y, 
            (m_dotFieldCount.z-1) / (float)(m_newChunkSize.z) * cellSize.z);
        if(downSampleRate<= 1)
            return dotField;
        InitBuffer();
        RunDownSampleComputeShader();
        newDotFieldBuffer.GetData(newDotField);
        ReleaseBuffer();
        return newDotField;
    }
}
