#ifndef RANDOMISLAND_HLSL
#define RANDOMISLAND_HLSL

#include "../Dot.hlsl"
#include "../SDFEssential.hlsl"
#include "../../Noise/FastSnoise.hlsl"
#include "../SDFOperations.hlsl"
#include "../SDFBasicShapes.hlsl"
#include "..\Hash.hlsl"

DotExpl duck(float3 pos,float3 seed)
{
    const float3 randVec =ClassicNoiseVec(seed*1000.1);
    const float scale = 0.5*randVec.x+0.5;
    const float3 hashVec = lcg_prng(seed);
    
    pos = sdf_pos_transform(pos, 2*(1-scale)*randVec*200-1, randVec, scale);
    
    const float3 baseSpherePos = sdf_pos_transform(pos, float3(-20, 0, 0), 0, float3(1.12f,0.8f,1));
    const DotExpl baseSphere = sdf_sphere(baseSpherePos, 100, float3(255,240,0)/255);
    
    const DotExpl headSphere = sdf_sphere(pos - float3(30, 100, 0), 60, float3(255,240,0)/255);

    const float3 mouseScale = float3(1,1,1.6);
    const float3 mousePos = sdf_pos_transform(pos,  float3(90, 100, 0), 0, mouseScale);
    const DotExpl mouseSphere = sdf_sphere(mousePos, 15, float3(255,100,0)/255);

    float3 eyepos1 = sdf_pos_transform(pos, float3(72, 127, -25), 0, float3(1,1,1));
    float3 eyepos2 = sdf_pos_transform(pos, float3(72, 127, 25), 0, float3(1,1,1));
    DotExpl eye1 = sdf_sphere(eyepos1, 8, int3(0,0,0));
    eye1 = smooth_colored_union(eye1,sdf_sphere(eyepos1-float3(6,0,-4), 2, float3(255,255,255)/255) ,2);
    DotExpl eye2 = sdf_sphere(eyepos2, 8, int3(0,0,0));
    eye2 = smooth_colored_union(eye2,sdf_sphere(eyepos2-float3(6,0,4), 2, float3(255,255,255)/255) ,2);

    const float3 wingPos1 = sdf_pos_transform(pos, float3(0, 20, 90), 0, float3(1.6,1,1));
    const float3 wingPos2 = sdf_pos_transform(pos, float3(0, 20, -90), 0, float3(1.6,1,1));
    const DotExpl wing1 = sdf_sphere(wingPos1, 30, float3(255,240,0)/255);
    const DotExpl wing2 = sdf_sphere(wingPos2, 30, float3(255,240,0)/255);

    DotExpl result = smooth_colored_union(baseSphere, headSphere, 30);
    result = smooth_colored_union(result, mouseSphere, 10);
    result = smooth_colored_union(result, eye1, 0);
    result = smooth_colored_union(result, eye2, 0);
    result = smooth_colored_union(result, wing1, 10);
    result = smooth_colored_union(result, wing2, 10);

    //result.color_impl = get_impl_color(get_expl_color(result.color_impl)*(randVec*255));
    
    return result;
}

DotExpl duck_world(float3 pos,float3 size)
{
    int3 id = round(pos/size);
    pos = pos- id*size;
    return duck(pos,id);
}


#endif