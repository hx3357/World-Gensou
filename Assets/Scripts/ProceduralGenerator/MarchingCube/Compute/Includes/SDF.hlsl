#define FLT_MAX 3.402823466e+38
#define SDF_MAX 100
#define SDF_MIN -SDF_MAX

float SDFNormalize(float value)
{
    if(value > SDF_MAX)
        return 1;
    if(value < SDF_MIN)
        return 0;
    return (value - SDF_MIN) / (SDF_MAX - SDF_MIN);
}

float sphereSDF(float3 origin, float radius, float3 position)
{
    return length(position - origin) - radius;
}