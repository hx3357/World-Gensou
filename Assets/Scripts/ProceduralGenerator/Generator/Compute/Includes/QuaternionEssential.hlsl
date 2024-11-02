#ifndef QUATERNIONESSENTIAL_HLSL
#define QUATERNIONESSENTIAL_HLSL

float4 quaternion_map_vec(float3 v1, float3 v2){
    v1 = normalize(v1);
    v2 = normalize(v2);
    float3 v = v1+v2;
    v = normalize(v);
    float4 q = 0;
    q.w = dot(v, v2);
    q.xyz = cross(v, v2);
    return q;
}

float4 quaternion_mul(float4 q1, float4 q2){
    float4 q = 0;
    q.w = q1.w*q2.w - dot(q1.xyz, q2.xyz);
    q.xyz = q1.w*q2.xyz + q2.w*q1.xyz + cross(q1.xyz, q2.xyz);
    return q;
}

#endif