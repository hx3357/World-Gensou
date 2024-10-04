#ifndef FRACTALNOISE_HLSL
#define FRACTALNOISE_HLSL

#include "noise.hlsl"

float fractalNoise(float3 pos,int ocatveCount,float lacunarity,float persistance)
{
    float sum = 0;
    float freq = 1;
    float amp = 1;
    float max = 0;
    for(int i=0;i<ocatveCount;i++)
    {
        sum += ClassicNoise(pos*freq)*amp;
        max += amp;
        freq *= lacunarity;
        amp *= persistance;
    }
    return sum/max;
}

#endif