#ifndef DEBUGWORLD_HLSL
#define DEBUGWORLD_HLSL

#include "../SDFEssential.hlsl"
#include "../../Noise/FastSnoise.hlsl"
#include "../SDFOperations.hlsl"
#include "../SDFBasicShapes.hlsl"
#include "../hash.hlsl"
#include "../Dot.hlsl"

float3 debug_repeat_2d(float3 pos,float2 size,out int3 id)
{
    float2 pos2d = pos.xz - size/2 - floor(pos.xz /size)*size;
    id = int3(floor(pos.x/size.x),0,floor(pos.z/size.y));
    return float3(pos2d.x,pos.y,pos2d.y);
}

DotExpl octave_sdf(float3 pos,float baseRadius,float height)
{
    int3 id;
    pos = debug_repeat_2d(pos, float2(baseRadius,baseRadius) * 2,id);
    return sdf_cylinder(pos, baseRadius* (0.5 * simple_hash(id) + 0.6), height, float3(1,1,1));
}

DotExpl cylinder_city_sdf(float3 pos, float baseRadius,float height,int octaves,float lacunarity,float gain)
{
    DotExpl sum = sdf_cylinder(pos, baseRadius, height, float3(1,1,1));
    float amp = 1;
    float freq = 1;
    for (int i = 0; i < octaves; i++)
    {
        sum = disform(sum, 0.1*amp);
        sum = smooth_colored_union(sum, octave_sdf(pos*freq,baseRadius*amp,height), 0.3*amp);
        amp *= gain;
        freq *= lacunarity;
    }
    return sum;
}

DotExpl fbm_shape_sdf(float3 pos, float3 origin,float baseRadius,float height,int octaves,float lacunarity,float gain)
{
    DotExpl sum = sdf_cylinder(pos - origin, baseRadius, height, float3(1,1,1));
    float amp = 1;
    float freq = 1;
    float3 curPos = pos;
    for (int i = 0; i < octaves; i++)
    {
        curPos = sdf_pos_translate(curPos, float3(37.2, 0, 47.6)*amp);
        DotExpl octave = octave_sdf(curPos, 1.2 * baseRadius*amp,height);
        octave = smooth_colored_intersection(octave,disform(sum, 0.4 *baseRadius*amp), 0.3*baseRadius*amp);
        sum = smooth_colored_union(sum, octave, 0.3*baseRadius*amp);
       // sum = op_round(sum, 0.1*baseRadius);
        amp *= gain;
        freq *= lacunarity;
    }
    sum.color_expl = lerp(GREEN,WHITE,(-pos.y + origin.y + 1.2*height)/height);
    return sum;
}

float neo_fbm_shape_sdf(float3 pos,float3 hash,float baseRadius,float height,int octaves,float lacunarity,float gain)
{
    float r = baseRadius,R = baseRadius * 1.2;
    int c = 3;
    float sum = SDF_MAX;
    float3 origin = 0;
    for(int i =0;i<octaves;i++)
    {
        float3 curOrigin = 0;
        float octave = SDF_MAX;
        for(int j = 0;j<c;j++)
        {
            float theta = simple_hash(hash + 58.6*float3(i,j,0)) * 2 * PI;
            float3 curPos = float3(origin.x+ r * cos(theta),origin.y,origin.z + r * sin(theta));
            curOrigin += curPos;
            octave = smooth_union(octave, sdf_inf_cylinder(pos - curPos,R), 0.3*R);
        }
        float expandShape = disform(sum,0.3*R);
        expandShape = smooth_intersection(expandShape,octave,0.3*R);
        sum = smooth_union(sum,expandShape,0.3*R);
        origin = curOrigin / c;
        r = (1+(1-gain))*r;
        R = gain * R;
        c = floor(clamp(lacunarity * c,1,30));
    }
    sum = disform(sum,R/2);
    return sum;
}

DotExpl debug_island_cluster_sdf(float3 pos,float3 origin,int iterateCount,float baseRadius,float height)
{
    float sum = SDF_MAX;
    float lastAngle = 0;
    float3 nextPos[10];
    float3 avgOrigin = origin;
    nextPos[0] = origin;
    for(int i=1;i<iterateCount;i++)
    {
        const float nextAngle = lastAngle - PI/2 + simple_hash(origin + float3(i,0,46.9)) * PI;
        const float nextDistance = baseRadius * (1 * simple_hash(origin + float3(i,34.8,33.2)) + 2);
        origin += float3(nextDistance * cos(nextAngle),0,nextDistance * sin(nextAngle));
        nextPos[i] = origin;
        lastAngle = nextAngle;
        avgOrigin += origin;
    }
    avgOrigin /= iterateCount;
    const float3 basicFbmShapePos = sdf_pos_islandlize(pos,avgOrigin,0);
    for(int i=0;i<iterateCount;i++)
    {
        sum = smooth_union(sum,
            neo_fbm_shape_sdf(basicFbmShapePos - nextPos[i],nextPos[i], 200, height, 8,1.2,0.9),
            1);
    }
    sum = max(sum,-sdf_plane(pos,-height));
    DotExpl result = create_dot_expl(sum,GRAY);
    result = smooth_colored_intersection(result,sdf_plane(pos,height,GREEN),baseRadius);
    return result;
}

DotExpl debug_world(float3 pos)
{
    DotExpl basicFbmShape = INIT_DOT_EXPL;
    basicFbmShape = colored_union(basicFbmShape,
        debug_island_cluster_sdf(pos,float3(0,0,0), 1, 100, 200));
    return basicFbmShape;
}

#endif
