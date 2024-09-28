#ifndef IMPLCOLOR_HLSL
#define IMPLCOLOR_HLSL

int get_impl_color(int3 color)
{
    return color.x | color.y  << 8| color.z << 16;
}

int3 get_expl_color(int color)
{
    return int3(color & 0xFF, color >> 8 & 0xFF, color >> 16 & 0xFF );
}

int lerp_impl_color(const int a,const int b,const float t)
{
    const float3 colorA = get_expl_color(a);
    const float3 colorB = get_expl_color(b);
    const float3 result = lerp(colorA, colorB, t);
    return get_impl_color(result);
}

#endif
