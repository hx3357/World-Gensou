#ifndef IMPLCOLOR_HLSL
#define IMPLCOLOR_HLSL

#define GREEN float3(0,1,0)
#define RED float3(1,0,0)
#define BLUE float3(0,0,1)
#define YELLOW float3(1,1,0)
#define CYAN float3(0,1,1)
#define MAGENTA float3(1,0,1)
#define WHITE float3(1,1,1)
#define BLACK float3(0,0,0)
#define GRAY float3(0.5,0.5,0.5)

int get_impl_color(float3 color)
{
    return int(clamp(color.x*255,0,255))& 0xFF | (int(clamp(color.y*255,0,255))& 0xFF) << 8 | (int(clamp(color.z*255,0,255))& 0xFF) << 16;
}

float3 get_expl_color(int color)
{
    return float3(color & 0xFF,color >> 8 & 0xFF, color >> 16 & 0xFF)/255;
}

int lerp_impl_color(const int a,const int b,const float t)
{
    const float3 colorA = get_expl_color(a);
    const float3 colorB = get_expl_color(b);
    const float3 result = lerp(colorA, colorB, t);
    return get_impl_color(result);
}

#endif
