#ifndef PRNG_HLSL
#define PRNG_HLSL

float3 lcg_prng(float3 vec)
{
    float3 result = (vec*1664525+1013904223)%4294967296./4294967296.;
    return result;
}

#endif