#ifndef HASH_HLSL
#define HASH_HLSL

float3 lcg_prng(float3 vec)
{
    float3 result = (vec*1664525+1013904223)%4294967296./4294967296.;
    return result;
}

uint3 pcg3d(uint3 v)
{
    v = v * 1664525u + 1013904223u;   

    v.x += v.y*v.z;
    v.y += v.z*v.x;
    v.z += v.x*v.y;

    v ^= v>>16u;

    v.x += v.y*v.z;
    v.y += v.z*v.x;
    v.z += v.x*v.y;

    return v;
}

// Hash function from H. Schechter & R. Bridson, goo.gl/RXiKaH
uint HRHash(uint s)
{
    s ^= 2747636419u;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    return s;
}

float HRHashf(uint s)
{
    return float(HRHash(s)) / 4294967296.0;
}

float simple_hash(float3 vec)
{
    float3 smallValue = sin(vec);
    float random = dot(smallValue, float3(12.9898, 78.233, 37.719));
    random = frac(sin(random) * 143758.5453);
    return random;
}

#endif