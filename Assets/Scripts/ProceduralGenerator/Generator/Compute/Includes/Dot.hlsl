#ifndef DOT_HLSL
#define DOT_HLSL

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

#endif