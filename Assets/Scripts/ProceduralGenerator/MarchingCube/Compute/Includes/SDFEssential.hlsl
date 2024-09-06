#include "..\Noise\noise.hlsl"
#include "..\Noise\simplexNoise3D.hlsl"

#define FLT_MAX 3.402823466e+38
#define SDF_MAX 100.

//SDF operations

//Linearly normalize the SDF value to 0-1
float SDFLinearNormalize(float value,float isoSurface)
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

float SDFBinaryNormalize(float value,float isoLevel)
{
    return value < isoLevel ? 0 : 1;
}

//Normalize the SDF value to 0-1
//Not perfect because the isosurface should be between 0.2 and 0.8 but it just works
float SDFParabolaNormalize(float value,float isoLevel)
{
    value = clamp(value,-SDF_MAX,SDF_MAX);
    return clamp((1-2*isoLevel)/(2*SDF_MAX*SDF_MAX)*value*value + 1/(2*SDF_MAX)*value + isoLevel,0,1);
}

float smoothUnion(float a, float b, float k)
{
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return lerp(b, a, h) - k * h * (1.0 - h);
}

float smoothSubtraction(float a, float b, float k)
{
    float h = clamp(0.5 - 0.5 * (b + a) / k, 0.0, 1.0);
    return lerp(b, -a, h) + k * h * (1.0 - h);
}

float smoothIntersection(float a, float b, float k)
{
    float h = clamp(0.5 - 0.5 * (b - a) / k, 0.0, 1.0);
    return lerp(b, a, h) + k * h * (1.0 - h);
}

float3 sdfTranslate(float3 position,float3 translation)
{
    return position-translation;
}

float3 sdfRotateAround(float3 position,float3 rotation,float3 center)
{
    float3x3 rotateInvertMatrix = float3x3(
        float3(cos(rotation.y) * cos(rotation.z), cos(rotation.y) * sin(rotation.z), -sin(rotation.y)),
        float3(sin(rotation.x) * sin(rotation.y) * cos(rotation.z) - cos(rotation.x) * sin(rotation.z), sin(rotation.x) * sin(rotation.y) * sin(rotation.z) + cos(rotation.x) * cos(rotation.z), sin(rotation.x) * cos(rotation.y)),
        float3(cos(rotation.x) * sin(rotation.y) * cos(rotation.z) + sin(rotation.x) * sin(rotation.z), cos(rotation.x) * sin(rotation.y) * sin(rotation.z) - sin(rotation.x) * cos(rotation.z), cos(rotation.x) * cos(rotation.y))
    );
    return mul(rotateInvertMatrix, position - center) + center;
}



//Utility functions

float getPerlinNoiseDisplacement(float3 position, float scale, float3 randomOffset)
{
    return (1 - ClassicNoise(position / scale + randomOffset));
}

float getSinDisplacement(float3 p, float scale)
{
    return sin(scale*p.x)*sin(scale*p.y)*sin(scale*p.z);
}

float3 cartesianToSpherical(float3 p)
{
    float r = length(p);
    if(p.x==0)
        p.x = 0.0001;
    float theta = atan2(p.z, p.x);
    float phi = acos(p.y / r);
    return float3(r, theta, phi);
}

//Basic SDF functions

float sphereSDF(float3 origin, float radius, float3 position)
{
    return length(position - origin) - radius;
}

float boxSDF(float3 origin, float3 size, float3 position)
{
    float3 p = position - origin;
    float3 q = abs(p) - size;
    return length(max(q,0.))+min(max(q.x,max(q.y,q.z)),0.);
}

float boxSDFBy2Points(float3 p1, float3 p2, float2 faceSize,float3 position)
{
    float3 origin = (p1+p2)/2;
    float y = length(p1-p2);
    float3 dir =p1-p2;
    float3 sphericalVec = cartesianToSpherical(dir);
    float3 rotation = float3(sphericalVec.z,sphericalVec.y,0);
    float3 _pos = sdfRotateAround(position,rotation,origin);
    return boxSDF(origin,float3(faceSize.x,y,faceSize.y),_pos);
}


//Island sdf
float basicAstoroidSDF(float3 origin,float3 position,float radius,float noiseScale,float noiseAmp ,float3 randomOffset)
{
    return sphereSDF(origin, radius, position)+
       noiseAmp* getPerlinNoiseDisplacement(position, noiseScale,randomOffset);
}

// float mixedArtifactsSDF(float3 origin,float3 position,float size,float noiseScale,float noiseAmp ,float3 randomOffset)
// {
//     float deviation = snoise(origin)*size;
//     float k = 2;
//     float3 p[] = {
//         float3(1,k,1)*deviation,
//         float3(-1,k,1)*deviation,
//         float3(1,-k,1)*deviation,
//         float3(-1,-k,1)*deviation,
//         float3(1,k,-1)*deviation,
//         float3(-1,k,-1)*deviation,
//         float3(1,-k,-1)*deviation,
//         float3(-1,-k,-1)*deviation
//     };
//     k = 1/2;
//     float3 q[] = {
//         float3(1,k,1)*deviation,
//         float3(-1,k,1)*deviation,
//         float3(1,-k,1)*deviation,
//         float3(-1,-k,1)*deviation,
//         float3(1,k,-1)*deviation,
//         float3(-1,k,-1)*deviation,
//         float3(1,-k,-1)*deviation,
//         float3(-1,-k,-1)*deviation
//     };
//
//     for(int i=0;i<8;i++)
//     {
//         
//     }
//     
//     return smoothUnion(sphereSDF())
// }