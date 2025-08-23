#include "CustomColor.hlsl"

float4 SoftmaxFloat4(float4 v, float temperature)
{
    const float invT = 1.0 / max(temperature, 1e-6);
    const float maxVal = max(max(v.x, v.y), max(v.z, v.w));
    v = (v - maxVal) * invT;
    float4 expVal = exp(v);
    const float sumVal = expVal.x + expVal.y + expVal.z + expVal.w;
    return expVal / sumVal;
}

float4 Top2MaskFloat4(float4 s)
{
    float first = max(max(s.x, s.y), max(s.z, s.w));
    float second = -3.402823e+38;
    if (s.x != first) second = max(second, s.x);
    if (s.y != first) second = max(second, s.y);
    if (s.z != first) second = max(second, s.z);
    if (s.w != first) second = max(second, s.w);

    return float4(
        (s.x >= second) ? 1.0 : 0.0,
        (s.y >= second) ? 1.0 : 0.0,
        (s.z >= second) ? 1.0 : 0.0,
        (s.w >= second) ? 1.0 : 0.0
    );
}

float4 Top3MaskFloat4(float4 s)
{
    float minVal = min(min(s.x, s.y), min(s.z, s.w));
    return float4(
        (s.x > minVal) ? 1.0 : 0.0,
        (s.y > minVal) ? 1.0 : 0.0,
        (s.z > minVal) ? 1.0 : 0.0,
        (s.w > minVal) ? 1.0 : 0.0
    );
}

void CalculateTerrainWeight_float(float3 dir,float temperature,float2 blendBound,out float4 weights)
{
    float4 rawWeights = max(float4(dot(dir,GRASS),dot(dir,SAND),dot(dir,ROCK),dot(dir,METAL)),0);
    //rawWeights = max(rawWeights,Top3MaskFloat4(rawWeights));
    weights = smoothstep(blendBound.x,blendBound.y,SoftmaxFloat4(rawWeights,temperature));
}

