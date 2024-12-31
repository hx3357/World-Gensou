using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class GrassRenderer : MonoSingleton<GrassRenderer>, IGrassRenderer
{
    public float boundSize = int.MaxValue;
    public Material grassMaterial;
    public uint maxInstanceCount = 10000;
    public ComputeShader grassComputeShader;
    public Camera mainCamera;
    [InspectorLabel("Grass properties")] 
    // Sphere coordinate
    public Vector3 windDir;
    public float windStrength;
    public float windlessBend;
    public float windlessSpread;
    public float windlessSwingStrength;
    public float windlessSwingSpeed;
    public float grassBaseHeight;

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
    private IGrassRenderer _grassRendererImplementation;
    private static readonly int Time1 = Shader.PropertyToID("time");
    private static readonly int WindDirection = Shader.PropertyToID("wind_direction");
    private static readonly int WindStrength = Shader.PropertyToID("wind_strength");
    private static readonly int WindlessBend = Shader.PropertyToID("windless_bend");
    private static readonly int WindlessSpread = Shader.PropertyToID("windless_spread");
    private static readonly int WindlessSwingStrength = Shader.PropertyToID("windless_swing_strength");
    private static readonly int WindlessSwingSpeed = Shader.PropertyToID("windless_swing_speed");
    private static readonly int GrassBaseHeight = Shader.PropertyToID("grass_base_height");
    private static readonly int InteractPositions = Shader.PropertyToID("interact_positions");
    private static readonly int InteractRadius = Shader.PropertyToID("interact_radius");
    private static readonly int InteractableCount = Shader.PropertyToID("interactable_count");

    private struct GrassPrecomputeData
    {
        private Vector3 position;
    }

    private struct GrassInstanceData
    {
        private Vector3 position;
        private float height;
        private float width;
        private float darkness;
        private float angle_dir;
        private float bend;
        private Vector3 euler_rotation;
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
        commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1,
            GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        commandData[0].indexCountPerInstance = grassMesh.GetIndexCount(0);
        renderParams = new RenderParams(grassMaterial)
        {
            camera = mainCamera
        };

        inputGrassBuffer =
            new ComputeBuffer((int)maxInstanceCount, Marshal.SizeOf(typeof(GrassPrecomputeData)), ComputeBufferType.Structured);
        outputGrassBuffer = new ComputeBuffer((int)maxInstanceCount, Marshal.SizeOf(typeof(GrassInstanceData)),
            ComputeBufferType.Structured);
        grassComputeShader.SetBuffer(0, InputGrassBuffer, inputGrassBuffer);
        grassComputeShader.SetBuffer(0, OutputGrassBuffer, outputGrassBuffer);
        grassComputeShader.GetKernelThreadGroupSizes(0, out var x, out _, out _);
        threadGroupSizeX = x;

        grassMaterial.SetBuffer(InstanceBuffer, outputGrassBuffer);
        grassMaterial.SetVector(GrassMeshSize, GrassMeshData.GrassMeshSize);
    }

    private void DisposeBuffer()
    {
        argsBuffer?.Dispose();
        outputGrassBuffer?.Dispose();
        inputGrassBuffer?.Dispose();
        commandBuffer?.Dispose();
    }

    public void DrawGrass(Vector3[] positions, Vector3 playerPos, GrassInteractable[] interactables)
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

        if (currentInstanceCount <= 0)
            return;
       
        inputGrassBuffer.SetData(positions);
        grassComputeShader.SetBuffer(0, InputGrassBuffer, inputGrassBuffer);
        grassComputeShader.SetBuffer(0, OutputGrassBuffer, outputGrassBuffer);
        grassComputeShader.SetFloat(Time1, Time.unscaledTime);
        grassComputeShader.SetVector(WindDirection, windDir);
        grassComputeShader.SetFloat(WindStrength, windStrength);
        grassComputeShader.SetFloat(WindlessBend, windlessBend);
        grassComputeShader.SetFloat(WindlessSpread, windlessSpread);
        grassComputeShader.SetFloat(WindlessSwingStrength, windlessSwingStrength);
        grassComputeShader.SetFloat(WindlessSwingSpeed, windlessSwingSpeed);
        grassComputeShader.SetFloat(GrassBaseHeight, grassBaseHeight);

        if (interactables != null)
        {
            (Vector4[] interactablePositions, float[] interactableRadius) = GrassInteractable.ToArray(interactables);
            grassComputeShader.SetVectorArray(InteractPositions,interactablePositions);
            grassComputeShader.SetFloats(InteractRadius,interactableRadius);
            grassComputeShader.SetInt(InteractableCount,interactables.Length);
        }
        else
        {
            grassComputeShader.SetVectorArray(InteractPositions,default);
            grassComputeShader.SetFloats(InteractRadius,default);
            grassComputeShader.SetInt(InteractableCount,0);
        }
        
        
        grassComputeShader.SetInt(GrassCount, (int)currentInstanceCount);
        grassComputeShader.Dispatch(0, 
            Mathf.CeilToInt(currentInstanceCount / (float)threadGroupSizeX), 1, 1);
        
        grassMaterial.SetBuffer(InstanceBuffer, outputGrassBuffer);

        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * boundSize);
        commandData[0].instanceCount = currentInstanceCount;
        commandBuffer.SetData(commandData);

        Graphics.RenderMeshIndirect(renderParams, grassMesh, commandBuffer);
    }
}