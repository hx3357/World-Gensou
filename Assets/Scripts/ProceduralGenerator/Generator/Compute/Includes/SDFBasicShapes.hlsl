#ifndef SDFBASICSHAPES_HLSL
#define SDFBASICSHAPES_HLSL

// The origin of the sdf space is always at the origin of the world space
// The origin parameter is only used for hash functions

#include "Dot.hlsl"
#include "CustomColor.hlsl"

DotExpl sdf_sphere(const float3 p, const float r, const float3 color, const int dot_type = DOT_TYPE_NORMAL)
{
    DotExpl dot;
    dot.w = length(p) - r;
    dot.color_expl = color;
    dot.dot_type = dot_type;
    return dot;
}

float sdf_sphere(const float3 p, const float r)
{
    return length(p) - r;
}

DotExpl sdf_cylinder(const float3 p, const float r, const float h, const float3 color,
                     const int dot_type = DOT_TYPE_NORMAL)
{
    const float2 d = abs(float2(length(p.xz), p.y)) - float2(r, h);
    DotExpl dot;
    dot.w = min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
    dot.color_expl = color;
    dot.dot_type = dot_type;
    return dot;
}

float sdf_cylinder(const float3 p, const float r, const float h)
{
    const float2 d = abs(float2(length(p.xz), p.y)) - float2(r, h);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

DotExpl sdf_inf_cylinder(const float3 p, const float r, const float3 color, const int dot_type = DOT_TYPE_NORMAL)
{
    return create_dot_expl(length(p.xz) - r, color, dot_type);
}

float sdf_inf_cylinder(const float3 p, const float r)
{
    return length(p.xz) - r;
}

DotExpl sdf_plane(const float3 p, const float h, const float3 color, const int dot_type = DOT_TYPE_NORMAL)
{
    return create_dot_expl(p.y - h, color, dot_type);
}

float sdf_plane(const float3 p, const float h)
{
    return p.y - h;
}

float sdf_box(const float3 position, float3 size)
{
    size /= 2;
    const float3 p = position;
    const float3 q = abs(p) - size;
    return length(max(q, 0.)) + min(max(q.x, max(q.y, q.z)), 0.);
}

float sdf_box_frame(float3 p, float3 b, float e)
{
    p = abs(p) - b;
    float3 q = abs(p + e) - e;
    return min(min(
                   length(max(float3(p.x, q.y, q.z), 0.0)) + min(max(p.x, max(q.y, q.z)), 0.0),
                   length(max(float3(q.x, p.y, q.z), 0.0)) + min(max(q.x, max(p.y, q.z)), 0.0)),
               length(max(float3(q.x, q.y, p.z), 0.0)) + min(max(q.x, max(q.y, p.z)), 0.0));
}

float sdf_torus(float3 p, float2 t)
{
    float2 q = float2(length(p.xz) - t.x, p.y);
    return length(q) - t.y;
}

float sdLink(float3 p, float le, float r1, float r2)
{
    float3 q = float3(p.x, max(abs(p.y) - le, 0.0), p.z);
    return length(float2(length(q.xy) - r1, q.z)) - r2;
}

#endif
