#include "../Includes/SDFEssential.hlsl"

float yAxisIslandShapeInvertTransform(float input,float smoothness)
{
    return input>=0?1:.7f;
}


float randomIslandSDFMode1(float3 pos, float3 islandPos, float3 randomOffset, float baseRadius=800, float caveRadius=5)
{

    float3 baseSpherePos = float3(pos.x,yAxisIslandShapeInvertTransform(pos.y,0.00001)*pos.y,pos.z);
    
    float baseSphere = sphereSDF(baseSpherePos, islandPos, baseRadius);
    
    float3 cutThroughCavePos = sdfRotateAround(pos,
        float3(snoise(islandPos)*2*PI, snoise(islandPos+float3(1,0,0))*2*PI, snoise(islandPos+float3(1,0,1))*2*PI),
        islandPos);
    float cutThroughCave = infCylinderSDF(cutThroughCavePos,islandPos, caveRadius);

    float value = smoothSubtraction(baseSphere,cutThroughCave, 30);

    value += 25 * getPerlinNoiseDisplacement(pos,50,randomOffset);
    
    return value;
}