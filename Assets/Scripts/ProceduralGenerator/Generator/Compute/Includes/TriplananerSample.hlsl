
void ComputeTriplanarUV_float(float3 worldPos, float3 worldNormal, float tiling, float sharpness,out float2 uvX,out float2 uvY,out float2 uvZ,out float3 weights)
{
    float3 n = normalize(worldNormal);
    float3 w = pow(abs(n), sharpness);
    w /= max(w.x + w.y + w.z, 1e-6);

    weights = w;
    
    uvX = worldPos.zy * tiling; 
    uvY = worldPos.xz * tiling;  
    uvZ = worldPos.xy * tiling; 
    
}