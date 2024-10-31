#ifndef SDFWEIRDSHAPES_HLSL
#define SDFWEIRDSHAPES_HLSL

#include "SDFBasicShapes.hlsl"
#include "SDFOperations.hlsl"
#include "Hash.hlsl"

DotExpl floating_rock_sdf(float3 pos, float3 origin, float baseRadius, float height, float maxRadius, float3 color)
{
    float3 rot_pos = sdf_pos_transform(pos,origin,origin,1);
    const float hash = simple_hash(origin);
    rot_pos = sdf_twist(rot_pos,0.01 * simple_hash(origin));
    float dot;
    if(hash < 0.5)
    {
        dot = sdf_box(rot_pos, float3(maxRadius/2, height/2, maxRadius/2)-20) - 10;
    }else 
    {
        dot = sdf_box_frame(rot_pos, float3(maxRadius/2, height/2, maxRadius/2)-20, height/25)-10;
    }
    
    
    return create_dot_expl(dot, lerp(WHITE,GRASS,clamp((pos.y - height/8 - origin.y )* 0.05 + 0.5,0,1)));
}

#endif