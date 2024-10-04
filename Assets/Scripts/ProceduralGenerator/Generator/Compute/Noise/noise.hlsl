#ifndef WGLNOISE_HLSL
#define WGLNOISE_HLSL
#include "common.hlsl"

float ClassicNoise_impl(float3 pi0, float3 pf0, float3 pi1, float3 pf1)
{
    pi0 = wglnoise_mod289(pi0);
    pi1 = wglnoise_mod289(pi1);

   const float4 ix = float4(pi0.x, pi1.x, pi0.x, pi1.x);
   const float4 iy = float4(pi0.y, pi0.y, pi1.y, pi1.y);
   const float4 iz0 = pi0.z;
   const float4 iz1 = pi1.z;

   const float4 ixy = wglnoise_permute(wglnoise_permute(ix) + iy);
   const float4 ixy0 = wglnoise_permute(ixy + iz0);
   const float4 ixy1 = wglnoise_permute(ixy + iz1);

    float4 gx0 = lerp(-1, 1, frac(floor(ixy0 / 7) / 7));
    float4 gy0 = lerp(-1, 1, frac(floor(ixy0 % 7) / 7));
    float4 gz0 = 1 - abs(gx0) - abs(gy0);

   const bool4 zn0 = gz0 < -0.01;
    gx0 += zn0 * (gx0 < -0.01 ? 1 : -1);
    gy0 += zn0 * (gy0 < -0.01 ? 1 : -1);

    float4 gx1 = lerp(-1, 1, frac(floor(ixy1 / 7) / 7));
    float4 gy1 = lerp(-1, 1, frac(floor(ixy1 % 7) / 7));
    float4 gz1 = 1 - abs(gx1) - abs(gy1);

   const bool4 zn1 = gz1 < -0.01;
    gx1 += zn1 * (gx1 < -0.01 ? 1 : -1);
    gy1 += zn1 * (gy1 < -0.01 ? 1 : -1);

    const float3 g000 = normalize(float3(gx0.x, gy0.x, gz0.x));
  const  float3 g100 = normalize(float3(gx0.y, gy0.y, gz0.y));
   const float3 g010 = normalize(float3(gx0.z, gy0.z, gz0.z));
   const float3 g110 = normalize(float3(gx0.w, gy0.w, gz0.w));
   const float3 g001 = normalize(float3(gx1.x, gy1.x, gz1.x));
   const float3 g101 = normalize(float3(gx1.y, gy1.y, gz1.y));
   const float3 g011 = normalize(float3(gx1.z, gy1.z, gz1.z));
    const float3 g111 = normalize(float3(gx1.w, gy1.w, gz1.w));

   const float n000 = dot(g000, pf0);
   const float n100 = dot(g100, float3(pf1.x, pf0.y, pf0.z));
   const float n010 = dot(g010, float3(pf0.x, pf1.y, pf0.z));
   const float n110 = dot(g110, float3(pf1.x, pf1.y, pf0.z));
   const float n001 = dot(g001, float3(pf0.x, pf0.y, pf1.z));
   const float n101 = dot(g101, float3(pf1.x, pf0.y, pf1.z));
   const float n011 = dot(g011, float3(pf0.x, pf1.y, pf1.z));
   const float n111 = dot(g111, pf1);

   const float3 fade_xyz = wglnoise_fade(pf0);
   const float4 n_z = lerp(float4(n000, n100, n010, n110),
                      float4(n001, n101, n011, n111), fade_xyz.z);
   const float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
   const float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
    return 1.46 * n_xyz;
}

// Classic Perlin noise
float ClassicNoise(float3 p)
{
    const float3 i = floor(p);
    const float3 f = frac(p);
    return ClassicNoise_impl(i, f, i + 1, f - 1);
}

// Classic Perlin noise, periodic variant
float PeriodicNoise(float3 p, float3 rep)
{
   const float3 i0 = wglnoise_mod(floor(p), rep);
   const float3 i1 = wglnoise_mod(i0 + 1, rep);
   const float3 f = frac(p);
    return ClassicNoise_impl(i0, f, i1, f - 1);
}

float3 ClassicNoiseVec(float3 p)
{
    return float3(ClassicNoise(p), ClassicNoise(p + 57.0), ClassicNoise(p + 113.0));
}

#endif