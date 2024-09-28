#ifndef UTILITY_HLSL
#define UTILITY_HLSL

// Get the index of a point in the dot field.
int get_point_index(const uint3 pos,const int3 size)
{
    return pos.x + pos.y * size.x + pos.z * size.x * size.y;
}

float3 get_point_position(const uint3 pos, const float cellSize)
{
    return pos * cellSize;
}

#endif