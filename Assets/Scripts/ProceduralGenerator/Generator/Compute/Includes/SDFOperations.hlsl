#ifndef SDFOPERATIONS_HLSL
#define SDFOPERATIONS_HLSL

#define MINVALUE .01

#include "Dot.hlsl"
#include "ImplColor.hlsl"

Dot colored_union(Dot a, Dot b)
{
   Dot result;
   if(a.w<b.w)
   {
      result.w = a.w;
      result.color_impl = a.color_impl;
   }else
   {
      result.w = b.w;
      result.color_impl = b.color_impl;
   }
   return result;
}

Dot colored_intersection(Dot a, Dot b)
{
   Dot result;
   if(a.w>b.w)
   {
      result.w = a.w;
      result.color_impl = a.color_impl;
   }else
   {
      result.w = b.w;
      result.color_impl = b.color_impl;
   }
   return result;
}

Dot smooth_colored_union(Dot a, Dot b, float k)
{
   Dot result;
   const float h = clamp(0.5 + 0.5 * (b.w - a.w) / k, 0.0, 1.0);
   result.w = lerp(b.w, a.w, h) - k * h * (1.0 - h);
   result.color_impl = lerp_impl_color(b.color_impl, a.color_impl, h);
   return result;
}

Dot smooth_colored_intersection(Dot a, Dot b, float k)
{
   Dot result;
   const float h = clamp(0.5 - 0.5 * (b.w - a.w) / k, 0.0, 1.0);
   result.w = lerp(b.w, a.w, h) + k * h * (1.0 - h);
   result.color_impl = lerp_impl_color(b.color_impl, a.color_impl, h);
   return result;
}

Dot smooth_colored_subtraction(Dot a, Dot b, float k)
{
   Dot result;
   const float h = clamp(0.5 - 0.5 * (a.w + b.w) / k, 0.0, 1.0);
   result.w = lerp(a.w, -b.w, h) + k * h * (1.0 - h);
   result.color_impl = a.color_impl;
   return result;
}

//Position operations

float3 sdf_pos_translate(float3 position,float3 translation)
{
   return position-translation;
}

float3 sdf_pos_rotate(float3 position,float3 rotation)
{
   const float3x3 rotateInvertMatrix = float3x3(
       float3(cos(rotation.y) * cos(rotation.z), cos(rotation.y) * sin(rotation.z), -sin(rotation.y)),
       float3(sin(rotation.x) * sin(rotation.y) * cos(rotation.z) - cos(rotation.x) * sin(rotation.z), sin(rotation.x) * sin(rotation.y) * sin(rotation.z) + cos(rotation.x) * cos(rotation.z), sin(rotation.x) * cos(rotation.y)),
       float3(cos(rotation.x) * sin(rotation.y) * cos(rotation.z) + sin(rotation.x) * sin(rotation.z), cos(rotation.x) * sin(rotation.y) * sin(rotation.z) - sin(rotation.x) * cos(rotation.z), cos(rotation.x) * cos(rotation.y))
   );
   return mul(rotateInvertMatrix, position);
}

float3 sdf_pos_scale(float3 pos,float3 scale)
{
   return pos/ scale;
}

float3 sdf_pos_transform(float3 pos, float3 origin, float3 rotation, float3 scale)
{
   return sdf_pos_scale(sdf_pos_rotate(pos-origin,rotation),scale);
}

float3 sdf_pos_bend( float3 p,float k)
{
   float c = cos(k*p.x);
   float s = sin(k*p.x);
   const float2x2 m = float2x2(c,-s,s,c);
   float3 q = float3(mul(m,p.xy),p.z);
   return q;
}

float3 sdf_pos_repeat(float3 p, float3 size)
{
   return p-round(p/size)*size;
}

float island_transform(float x)
{
   if(x>0)return 1;
   return -0.01*x+1;
}

float3 sdf_pos_islandlize(float3 p,float h)
{
   if(p.y>h)
      return p;
   float sliceScale = island_transform(p.y-h);
   return float3(p.x,0,p.z)*sliceScale+float3(0,p.y,0);
}

#endif