using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GrassRendererOldAPI : MonoSingleton<GrassRendererOldAPI>, IGrassRenderer
{
    public Material grassMaterial;
    public ComputeShader grassComputeShader;

    private float boundSize = int.MaxValue;
    private uint maxInstanceCount = 10000;

    private ComputeBuffer argsBuffer;
    private ComputeBuffer outputGrassBuffer;
    private ComputeBuffer inputGrassBuffer;
    private static readonly int InstanceBuffer = Shader.PropertyToID("instance_buffer");
    private uint threadGroupSizeX;
    private Bounds bounds;
    private static readonly int GrassCount = Shader.PropertyToID("grass_count");

    private struct GrassPrecomputeData
    {
        private Vector3 position;

        public static int SizeOf => sizeof(float) * 3;
    }

    private struct GrassInstanceData
    {
        private Vector3 position;
        private float height;
        private float width;
        private float darkness;
        private float angle_dir;
        private float bend;
    }

    protected override void Awake()
    {
        base.Awake();
        bounds = new Bounds(Vector3.zero, Vector3.one * boundSize);
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
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        inputGrassBuffer =
            new ComputeBuffer((int)maxInstanceCount, GrassPrecomputeData.SizeOf, ComputeBufferType.Structured);
        outputGrassBuffer = new ComputeBuffer((int)maxInstanceCount, Marshal.SizeOf(typeof(GrassInstanceData)),
            ComputeBufferType.Structured);
        grassComputeShader.SetBuffer(0, "input_grass_buffer", inputGrassBuffer);
        grassComputeShader.SetBuffer(0, "output_grass_buffer", outputGrassBuffer);
        grassMaterial.SetBuffer(InstanceBuffer, outputGrassBuffer);
        grassComputeShader.GetKernelThreadGroupSizes(0, out var x, out _, out _);
        threadGroupSizeX = x;
    }

    private void DisposeBuffer()
    {
        argsBuffer.Dispose();
        argsBuffer = null;
        outputGrassBuffer.Dispose();
        outputGrassBuffer = null;
        inputGrassBuffer.Dispose();
        inputGrassBuffer = null;
    }

    public uint InstanceCount
    {
        get => maxInstanceCount;
        set => maxInstanceCount = value;
    }

    public void DrawGrass(Vector3[] positions, Vector3 playerPos)
    {
        int currentInstanceCount;
        if (positions.Length > maxInstanceCount)
        {
            positions = new ArraySegment<Vector3>(positions, 0, (int)maxInstanceCount).ToArray();
            currentInstanceCount = (int)maxInstanceCount;
        }
        else
        {
            currentInstanceCount = positions.Length;
        }

        if (currentInstanceCount <= 0) return;

        inputGrassBuffer.SetData(positions);
        grassComputeShader.SetInt(GrassCount, currentInstanceCount);

        grassComputeShader.Dispatch(0, Mathf.CeilToInt(currentInstanceCount / (float)threadGroupSizeX), 1, 1);

        argsBuffer.SetData(new uint[]
            { GrassMeshData.GrassMesh.GetIndexCount(0), (uint)currentInstanceCount, 0, 0, 0 });
        Graphics.DrawMeshInstancedIndirect(GrassMeshData.GrassMesh,
            0,
            grassMaterial,
            bounds,
            argsBuffer);
    }
}