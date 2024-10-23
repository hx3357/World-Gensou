#ifndef FRACTALNOISE_HLSL
#define FRACTALNOISE_HLSL


#include "noise.hlsl"
#include "FastSnoise.hlsl"



float fractalNoise(const float3 pos,int ocatveCount,float lacunarity,float persistance)
{
    float sum = 0;
    float freq = 1;
    float amp = 1;
    for(int i=0;i<ocatveCount;i++)
    {
        sum += ClassicNoise(pos*freq)*amp;
        freq *= lacunarity;
        amp *= persistance;
    }
    return sum/2 + 0.5;
}

#endif