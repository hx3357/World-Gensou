using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PerlinNoiseScalerFieldGenerator3D_GPU : GPUScalerFieldGenerator
{
   private readonly int seed;
   private readonly float scale;
   private readonly float isoLevel;
   private Vector3 randomOffset;
   private System.Random prng;
   
   private static readonly int Scale = Shader.PropertyToID("scale");
   private static readonly int IsoLevel = Shader.PropertyToID("isoLevel");
   private static readonly int RandomOffset = Shader.PropertyToID("randomOffset");
   
   
   private static readonly int IsConcreteFlagBuffer = Shader.PropertyToID("isConcreteFlagBuffer");
   private static readonly int IsAirFlagBuffer = Shader.PropertyToID("isAirFlagBuffer");

   public PerlinNoiseScalerFieldGenerator3D_GPU(ComputeShader m_cs,int m_seed,float m_scale,float m_isoLevel) : base(m_cs)
   {
      seed = m_seed;
      scale = m_scale;
      isoLevel = m_isoLevel;
      prng = new System.Random(seed);
      randomOffset = new Vector3(prng.Next(-100000, 100000), prng.Next(-100000, 100000), prng.Next(-100000, 100000));
   }

   protected override void InitBuffer()
   {
      base.InitBuffer();
      ComputeBuffer isConcreteFlagBuffer = new ComputeBuffer(1, sizeof(int));
      isConcreteFlagBuffer.SetData(new []{1});
      buffers.Add(isConcreteFlagBuffer);
      ComputeBuffer isAirFlagBuffer = new ComputeBuffer(1, sizeof(int));
      isAirFlagBuffer.SetData(new[]{1});
      buffers.Add(isAirFlagBuffer);
   }

   protected override void GenerateRequest(ScalerFieldRequestData scalerFieldRequestData)
   {
      base.GenerateRequest(scalerFieldRequestData);
      AsyncGPUReadbackRequest isConcreteFlagBufferRequest = AsyncGPUReadback.Request(scalerFieldRequestData.buffers[1], 
         sizeof(int), 0);
      requests.Add(isConcreteFlagBufferRequest);
      AsyncGPUReadbackRequest isAirFlagBufferRequest = AsyncGPUReadback.Request(scalerFieldRequestData.buffers[2],
         sizeof(int), 0);
      requests.Add(isAirFlagBufferRequest);
   }

   protected override void SetComputeShaderParameters(ComputeShader m_cs,ScalerFieldRequestData scalerFieldRequestData,object[] parameters)
   {
      m_cs.SetFloat(Scale, scale);
      m_cs.SetFloat(IsoLevel, isoLevel);
      m_cs.SetVector(RandomOffset, randomOffset);
      m_cs.SetBuffer(0, IsConcreteFlagBuffer, scalerFieldRequestData.buffers[1]);
      m_cs.SetBuffer(0, IsAirFlagBuffer, scalerFieldRequestData.buffers[2]);
   }

   public override (bool,Dot[],bool) GetState (ref ScalerFieldRequestData scalerFieldRequestData,bool isNotGetDotfield = false)
   {
      AsyncGPUReadbackRequest isConcreteFlagBufferRequest =scalerFieldRequestData.requests[1];
      AsyncGPUReadbackRequest isAirFlagBufferRequest = scalerFieldRequestData.requests[2];
      if (!isConcreteFlagBufferRequest.done || !isAirFlagBufferRequest.done)
         return (false,null,false);
      
      return base.GetState(ref scalerFieldRequestData,isNotGetDotfield);
   }

   protected override bool GetEmptyState(ScalerFieldRequestData scalerFieldRequestData)
   {
      AsyncGPUReadbackRequest isConcreteFlagBufferRequest =scalerFieldRequestData.requests[1];
      AsyncGPUReadbackRequest isAirFlagBufferRequest = scalerFieldRequestData.requests[2];
      int[] isConcreteFlag = new int[1];
      isConcreteFlagBufferRequest.GetData<int>().CopyTo(isConcreteFlag);
      int[] isAirFlag = new int[1];
      isAirFlagBufferRequest.GetData<int>().CopyTo(isAirFlag);
      return isConcreteFlag[0] == 1 || isAirFlag[0] == 1;
   }
}
