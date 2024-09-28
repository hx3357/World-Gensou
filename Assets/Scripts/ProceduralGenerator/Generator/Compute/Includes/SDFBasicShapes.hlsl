#ifndef SDFBASICSHAPES_HLSL
#define SDFBASICSHAPES_HLSL

#include "Dot.hlsl"
#include "ImplColor.hlsl"

Dot colored_sphere(const float3 p, const float r, const int3 color)
{
    Dot dot;
    dot.w = length(p) - r;
    dot.color_impl = get_impl_color(color);
    return dot;
}

#endif