using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class Oscillator : MonoBehaviour
{
   public bool isEnabled; 
   
   [Range(0,0.3f)]
   public double gain ;
   [Range(20,20000)]
   public double frequency = 440;

   [Range(-1, 1)] public float pan;
   
   public enum WaveType
   {
      Sine,
      Square,
      Sawtooth,
      Triangle
   }
   public WaveType waveType;
   
   [Range(0,1)]
   public float lerpParam=1;
   public bool isSimpleLerp = false;
   
   private const int SAMPLE_RATE = 48000;
   private double phase;
   void OnAudioFilterRead(float[] data,int channels)
   {
      if (!isEnabled) return;
      float w = 2 * Mathf.PI * (float)frequency / SAMPLE_RATE;
      for(int i=0;i<data.Length;i+=channels)
      {
         if(isSimpleLerp)
            data[i] = (float)gain * Mathf.Lerp(GetWaveTableValue(WaveType.Sawtooth, (float)phase), GetWaveTableValue(WaveType.Square, (float)phase), lerpParam);
         else
            data[i] = (float)gain * (lerpParam*GetWaveTableValue(WaveType.Square, (float)phase)+1-lerpParam)*
                      ((1-lerpParam)*GetWaveTableValue(WaveType.Sawtooth, (float)phase)+lerpParam);
         phase += w;

         switch (channels)
         {
            case 1:
               //Mono
               //do nothing
               break;
            case 2:
               //Stereo
               data[i + 1] = (1+pan)/2*data[i];
               data[i] = (1-pan)/2*data[i];
               break;
         }
         
         if (phase > (2 * Mathf.PI))
         {
            phase = 0;
         }
      }
   }
   
   protected virtual float GetWaveTableValue(WaveType m_waveType ,float m_phase)
   {
      switch (m_waveType)
      {
         case WaveType.Sine:
            return Mathf.Sin(m_phase);
         case WaveType.Square:
            if(m_phase<Mathf.PI)
               return 1;
            else
               return -1;
         case WaveType.Sawtooth:
            return 2 * (m_phase / (2 * Mathf.PI)) - 1;
         case WaveType.Triangle:
            if(phase<Mathf.PI)
               return 2 * (m_phase / (2 * Mathf.PI)) - 1;
            else
               return 1 - 2 * (m_phase / (2 * Mathf.PI));
      }

      return 0;
   }
   
}
