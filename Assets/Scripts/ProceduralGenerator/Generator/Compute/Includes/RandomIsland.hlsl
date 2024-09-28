#ifndef RANDOMISLAND_HLSL
#define RANDOMISLAND_HLSL

#include "SDFEssential.hlsl"
#include "../Noise/FastSnoise.hlsl"
#include "SDFOperations.hlsl"
#include "SDFBasicShapes.hlsl"
#include "Prng.hlsl"


//Not exact
float3 island_shape_transform(float3 p,float amp)
{
    return float3(p.x,(p.y>=0?1:1/amp)*p.y,p.z); 
}

//Base island shape
float base_island_sdf(float3 pos, float3 islandPos, float baseRadius,float cutHeight,float smoothValue=50)
{
    const float3 baseSpherePos = island_shape_transform(pos,1);
    return cut_sphere_sdf(baseSpherePos, islandPos, baseRadius,cutHeight)-smoothValue;
}



float random_island_sdf_mode1(float3 pos, float3 islandPos, float3 randomOffset, float baseRadius=500, float caveRadius=50)
{
    const float baseSphere = base_island_sdf(pos, islandPos, baseRadius, 50);

    float3 baseSphereNormal;
    //CalcNormal(baseIslandSDF,baseSphereNormal,pos,islandPos,baseRadius,50)
    
    const float3 cutThroughCavePos = sdf_pos_rotate(pos,
        float3(f_snoise(islandPos)*2*PI, f_snoise(islandPos+float3(1,0,0))*2*PI, f_snoise(islandPos+float3(1,0,1))*2*PI));
    float cutThroughCave = inf_cylinder_sdf(cutThroughCavePos-islandPos,islandPos, caveRadius);
    cutThroughCave += 10 * get_perlin_noise_displacement(pos,50,randomOffset);

    const float value = smooth_subtraction(baseSphere,cutThroughCave, 50);

    
    return value;
}

Dot duck(float3 pos,float3 seed)
{
    const float3 randVec =ClassicNoiseVec(seed*1000.1);
    const float scale = 0.5*randVec.x+0.5;
    const float3 hashVec = lcg_prng(seed);
    
    pos = sdf_pos_transform(pos, 2*(1-scale)*randVec*200-1, randVec, scale);
    
    const float3 baseSpherePos = sdf_pos_transform(pos, float3(-20, 0, 0), 0, float3(1.12f,0.8f,1));
    const Dot baseSphere = colored_sphere(baseSpherePos, 100, int3(255,240,0));
    
    const Dot headSphere = colored_sphere(pos - float3(30, 100, 0), 60, int3(255,240,0));

    const float3 mouseScale = float3(1,1,1.6);
    const float3 mousePos = sdf_pos_transform(pos,  float3(90, 100, 0), 0, mouseScale);
    const Dot mouseSphere = colored_sphere(mousePos, 15, int3(255,100,0));

    float3 eyepos1 = sdf_pos_transform(pos, float3(72, 127, -25), 0, float3(1,1,1));
    float3 eyepos2 = sdf_pos_transform(pos, float3(72, 127, 25), 0, float3(1,1,1));
    Dot eye1 = colored_sphere(eyepos1, 8, int3(0,0,0));
    eye1 = smooth_colored_union(eye1,colored_sphere(eyepos1-float3(6,0,-4), 2, int3(255,255,255)) ,2);
    Dot eye2 = colored_sphere(eyepos2, 8, int3(0,0,0));
    eye2 = smooth_colored_union(eye2,colored_sphere(eyepos2-float3(6,0,4), 2, int3(255,255,255)) ,2);

    const float3 wingPos1 = sdf_pos_transform(pos, float3(0, 20, 90), 0, float3(1.6,1,1));
    const float3 wingPos2 = sdf_pos_transform(pos, float3(0, 20, -90), 0, float3(1.6,1,1));
    const Dot wing1 = colored_sphere(wingPos1, 30, int3(255,240,0));
    const Dot wing2 = colored_sphere(wingPos2, 30, int3(255,240,0));

    Dot result = smooth_colored_union(baseSphere, headSphere, 30);
    result = smooth_colored_union(result, mouseSphere, 10);
    result = smooth_colored_union(result, eye1, 0);
    result = smooth_colored_union(result, eye2, 0);
    result = smooth_colored_union(result, wing1, 10);
    result = smooth_colored_union(result, wing2, 10);

    //result.color_impl = get_impl_color(get_expl_color(result.color_impl)*(randVec*255));
    
    return result;
}

Dot duck_world(float3 pos,float3 size)
{
    int3 id = round(pos/size);
    pos = pos- id*size;
   // pos = sdf_pos_islandlize(pos,0);
    return duck(pos,id);
}


#endif