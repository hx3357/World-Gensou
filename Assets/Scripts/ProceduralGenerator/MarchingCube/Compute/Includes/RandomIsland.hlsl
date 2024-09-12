#ifndef RANDOMISLAND_HLSL
#define RANDOMISLAND_HLSL

#include "../Includes/SDFEssential.hlsl"
#include "../Noise/FastSnoise.hlsl"

//Not exact
float3 islandShapeTransform(float3 p,float amp)
{
    return float3(p.x,(p.y>=0?1:1/amp)*p.y,p.z); 
}

//Base island shape
float baseIslandSDF(float3 pos, float3 islandPos, float baseRadius,float cutHeight,float smoothValue=50)
{
    const float3 baseSpherePos = islandShapeTransform(pos,1);
    return cutSphereSDF(baseSpherePos, islandPos, baseRadius,cutHeight)-smoothValue;
}



float randomIslandSDFMode1(float3 pos, float3 islandPos, float3 randomOffset, float baseRadius=500, float caveRadius=50)
{
    const float baseSphere = baseIslandSDF(pos, islandPos, baseRadius, 50);

    float3 baseSphereNormal;
    //CalcNormal(baseIslandSDF,baseSphereNormal,pos,islandPos,baseRadius,50)
    
    const float3 cutThroughCavePos = sdfRotateAround(pos,
        float3(f_snoise(islandPos)*2*PI, f_snoise(islandPos+float3(1,0,0))*2*PI, f_snoise(islandPos+float3(1,0,1))*2*PI),
        islandPos);
    float cutThroughCave = infCylinderSDF(cutThroughCavePos,islandPos, caveRadius);
    cutThroughCave += 10 * getPerlinNoiseDisplacement(pos,50,randomOffset);

    const float value = smoothSubtraction(baseSphere,cutThroughCave, 50);

    
    return value;
}

#endif