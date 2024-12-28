using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class GrassRenderer: MonoSingleton<GrassRenderer>,IGrassRenderer
{
    public float boundSize = Int32.MaxValue;
    public Material grassMaterial;
    public uint maxInstanceCount = 10000;
    public ComputeShader grassComputeShader;
    public Camera mainCamera;
    [InspectorLabel("Grass properties")]
    public Vector3 windDir;
    public float windStrength;
    public float windlessBend;
    public float windlessSwingStrength;
    
    private RenderParams renderParams;
    private GraphicsBuffer commandBuffer;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    private Mesh grassMesh;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer outputGrassBuffer;
    private ComputeBuffer inputGrassBuffer;
    
    private uint threadGroupSizeX;
    private static readonly int InstanceBuffer = Shader.PropertyToID("instance_buffer");
    private static readonly int GrassCount = Shader.PropertyToID("grass_count");
    private static readonly int InputGrassBuffer = Shader.PropertyToID("input_grass_buffer");
    private static readonly int OutputGrassBuffer = Shader.PropertyToID("output_grass_buffer");
    private static readonly int GrassMeshSize = Shader.PropertyToID("grass_mesh_size");

    struct GrassPrecomputeData
    {
        Vector3 position;
        
        public static int SizeOf => sizeof(float) * 3;
    }

    struct GrassInstanceData
    {
        Vector3 position;
        float height;
        float width;
        float darkness;
        float angle_dir;
        float bend;
        Vector3 euler_rotation;
    }

    private void Start()
    {
        InitBuffer();
    }
    
    private void OnDestroy()
    {
        DisposeBuffer();
    }

    private void InitBuffer()
    {
        grassMesh = GrassMeshData.GrassMesh;
        commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        commandData[0].indexCountPerInstance = grassMesh.GetIndexCount(0);
        renderParams = new RenderParams(grassMaterial)
        {
            camera = mainCamera
        };
        
        inputGrassBuffer = new ComputeBuffer((int)maxInstanceCount, GrassPrecomputeData.SizeOf,ComputeBufferType.Structured);
        outputGrassBuffer = new ComputeBuffer((int)maxInstanceCount, Marshal.SizeOf(typeof(GrassInstanceData)),ComputeBufferType.Structured);
        grassComputeShader.SetBuffer(0, InputGrassBuffer, inputGrassBuffer);
        grassComputeShader.SetBuffer(0, OutputGrassBuffer, outputGrassBuffer);
        grassComputeShader.GetKernelThreadGroupSizes(0, out uint x, out _, out _);
        threadGroupSizeX = x;
        
        grassMaterial.SetBuffer(InstanceBuffer, outputGrassBuffer);
        grassMaterial.SetVector(GrassMeshSize, GrassMeshData.GrassMeshSize);
    }
    
    void DisposeBuffer()
    {
        argsBuffer?.Dispose();
        outputGrassBuffer?.Dispose();
        inputGrassBuffer?.Dispose();
        commandBuffer?.Dispose();
    }
    
    public void DrawGrass(Vector3[] positions,Vector3 playerPos)
    {
        uint currentInstanceCount;
        if (positions.Length > maxInstanceCount)
        {
            positions = new ArraySegment<Vector3>(positions, 0, (int)maxInstanceCount).ToArray();
            currentInstanceCount = maxInstanceCount;
        }
        else
        {
            currentInstanceCount = (uint)positions.Length;
        }
        
        if(currentInstanceCount<=0)
            return;
        
        inputGrassBuffer.SetData(positions);
        grassComputeShader.SetFloat("time", Time.unscaledTime);
        grassComputeShader.SetVector("wind_direction", windDir);
        grassComputeShader.SetFloat("wind_strength", windStrength);
        grassComputeShader.SetFloat("windless_bend", windlessBend);
        grassComputeShader.SetFloat("windless_swing_strength", windlessSwingStrength);
        grassComputeShader.SetInt(GrassCount, (int)currentInstanceCount);
        grassComputeShader.Dispatch(0, Mathf.CeilToInt(currentInstanceCount / (float)threadGroupSizeX), 1, 1);
        
        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * boundSize);
        commandData[0].instanceCount = currentInstanceCount;
        commandBuffer.SetData(commandData);
        
        Graphics.RenderMeshIndirect(renderParams,grassMesh,commandBuffer);
    }
}
