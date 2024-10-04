#ifndef DOT_HLSL
#define DOT_HLSL

#include "CustomColor.hlsl"

#define INIT_DOT_EXPL {SDF_MAX,float3(1,1,1)};

typedef struct
{
    float w;
    int color_impl;
} Dot;

int3 get_dot_color(const Dot dot)
{
    return int3(dot.color_impl & 0xFF, (dot.color_impl >> 8) & 0xFF, (dot.color_impl >> 16) & 0xFF);
}

void set_dot_color(inout Dot dot,const int3 color)
{
    dot.color_impl = color.x | (color.y << 8) | (color.z << 16);
}

void set_dot_value(inout Dot dot,const float w,const int3 color)
{
    dot.w = w;
    set_dot_color(dot, color);
}

Dot create_dot(const float w,const int3 color)
{
    Dot dot;
    dot.w = w;
    dot.color_impl = color.x | (color.y << 8) | (color.z << 16);
    return dot;
}

typedef struct
{
    float w;
    float3 color_expl;
} DotExpl;

DotExpl create_dot_expl(const float w,const float3 color)
{
    DotExpl dot;
    dot.w = w;
    dot.color_expl = color;
    return dot;
}

Dot convert_dot_expl_to_dot(const DotExpl dot)
{
    Dot result;
    result.w = dot.w;
    result.color_impl = get_impl_color(dot.color_expl);
    return result;
}

#endif