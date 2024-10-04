#ifndef WHITE_NOISE_HLSL
#define WHITE_NOISE_HLSL

float whiteNoise(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719)){
    float3 smallValue = sin(value);
    float random = dot(smallValue, dotDir);
    random = frac(sin(random) * 143758.5453);
    return random;
}

#endif
