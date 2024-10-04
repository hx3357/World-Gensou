#ifndef SDFESSENTIAL_HLSL
#define SDFESSENTIAL_HLSL
#include "..\Noise\fractalNoise.hlsl"

#define FLT_MAX 3.402823466e+38
#define SDF_MAX 100.
#define PI 3.14159265

//SDF operations

//Linearly normalize the SDF value to 0-1
float sdf_linear_normalize(float value,float isoSurface)
{
    if(isoSurface==0.5f)
        return clamp(1/(2*SDF_MAX)*value + isoSurface,0,1);
    if(value<0)
    {
        return clamp(isoSurface/SDF_MAX*value+isoSurface,0,1);
    }else
    {
        return clamp((1-isoSurface)/SDF_MAX*value+isoSurface,0,1);
    }
}

float sdf_binary_normalize(float value,float isoLevel)
{
    return value < isoLevel ? 0 : 1;
}

//Normalize the SDF value to 0-1
//Not perfect because the isosurface should be between 0.2 and 0.8 but it just works
float sdf_parabola_normalize(float value,float isoLevel)
{
    value = clamp(value,-SDF_MAX,SDF_MAX);
    return clamp((1-2*isoLevel)/(2*SDF_MAX*SDF_MAX)*value*value + 1/(2*SDF_MAX)*value + isoLevel,0,1);
}



//Why unity doesn't support this?????
#if 0
#define CalcNormal(sdfFunc,normal,pos,...) do \
{\
    const float h = 0.0001;\
    const float2 k = float2(1,-1);\
    normal = normalize( k.xyy*sdfFunc( pos + k.xyy*h ,__VA_ARGS__) + \
                      k.yyx*sdfFunc( pos + k.yyx*h ,__VA_ARGS__) + \
                      k.yxy*sdfFunc( pos + k.yxy*h ,__VA_ARGS__) + \
                      k.xxx*sdfFunc( pos + k.xxx*h ,__VA_ARGS__) );\
}while(0)
#endif


//Displacement functions

float get_perlin_noise_displacement(float3 position, float scale, float3 randomOffset)
{
    return (1 - ClassicNoise(position / scale + randomOffset));
}

float get_sin_displacement(float3 p, float scale)
{
    return sin(scale*p.x)*sin(scale*p.y)*sin(scale*p.z);
}

//Basic SDF functions

float sphere_sdf(float3 position, float3 origin, float radius)
{
    return length(position - origin) - radius;
}

float box_sdf(float3 position, float3 origin, float3 size)
{
    size /= 2;
   const float3 p = position - origin;
    float3 q = abs(p) - size;
    return length(max(q,0.))+min(max(q.x,max(q.y,q.z)),0.);
}

float inf_cylinder_sdf(float3 position, float3 origin, float radius)
{
    return length(position.xz-origin.xz) - radius;
}

float cut_sphere_sdf(float3 position, float3 origin,float r,float h)
{
    float3 p = position - origin;
    p = float3(p.x,-p.y,p.z);
    h*=-1;
    
    // sampling independent computations (only depend on shape)
    float w = sqrt(r*r-h*h);

    // sampling dependant computations
    float2 q = float2( length(p.xz), p.y );
    const float s = max( (h-r)*q.x*q.x+w*w*(h+r-2.0*q.y), h*q.x-w*q.y );
    return (s<0.0) ? length(q)-r :
           (q.x<w) ? h - q.y     :
                     length(q-float2(w,h));
}


//Island sdf
float basic_astoroid_sdf(float3 origin,float3 position,float radius,float noiseScale,float noiseAmp ,float3 randomOffset)
{
    return sphere_sdf(position, origin, radius)+
       noiseAmp* get_perlin_noise_displacement(position, noiseScale,randomOffset);
}

float bbasic_planet_sdf(float3 origin,float3 position,float radius,float noiseScale,float noiseAmp ,float3 randomOffset)
{
    return sphere_sdf(position, origin, radius)+
       noiseAmp* fractalNoise(position/noiseScale + randomOffset,4,3,0.5);
}
#endif
