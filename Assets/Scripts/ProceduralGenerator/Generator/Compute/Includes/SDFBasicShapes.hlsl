#ifndef SDFBASICSHAPES_HLSL
#define SDFBASICSHAPES_HLSL

#include "Dot.hlsl"
#include "CustomColor.hlsl"

DotExpl sdf_sphere(const float3 p, const float r, const float3 color)
{
    DotExpl dot;
    dot.w = length(p) - r;
    dot.color_expl = color;
    return dot;
}

float sdf_sphere(const float3 p, const float r)
{
    return length(p) - r;
}

DotExpl sdf_cylinder(const float3 p, const float r, const float h, const float3 color)
{
    const float2 d = abs(float2(length(p.xz), p.y)) - float2(r, h);
    DotExpl dot;
    dot.w = min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
    dot.color_expl = color;
    return dot;
}

float sdf_cylinder(const float3 p, const float r, const float h)
{
    const float2 d = abs(float2(length(p.xz), p.y)) - float2(r, h);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

DotExpl sdf_inf_cylinder(const float3 p, const float r, const float3 color)
{
    return create_dot_expl(length(p.xz) - r, color);
}

float sdf_inf_cylinder(const float3 p, const float r)
{
    return length(p.xz) - r;
}

DotExpl sdf_plane(const float3 p,const float h, const float3 color)
{
    return create_dot_expl(p.y - h, color);
}

float sdf_plane(const float3 p,const float h)
{
    return p.y - h;
}

#endif