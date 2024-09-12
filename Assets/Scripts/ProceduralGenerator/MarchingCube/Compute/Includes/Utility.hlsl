#ifndef UTILITY_HLSL
#define UTILITY_HLSL

// Get the index of a point in the dot field.
int GetPointIndex(const int x,const int y,const int z,int3 size)
{
    return x + y * size.x + z * size.x * size.y;
}
#endif