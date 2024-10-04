#ifndef SDFOPERATIONS_HLSL
#define SDFOPERATIONS_HLSL

#define MINVALUE .01

#include "Dot.hlsl"
#include "CustomColor.hlsl"

DotExpl colored_union(DotExpl a, DotExpl b)
{
   DotExpl result;
   if(a.w<b.w)
   {
      result.w = a.w;
      result.color_expl = a.color_expl;
   }else
   {
      result.w = b.w;
      result.color_expl = b.color_expl;
   }
   return result;
}

DotExpl colored_intersection(DotExpl a, DotExpl b)
{
   DotExpl result;
   if(a.w>b.w)
   {
      result.w = a.w;
      result.color_expl = a.color_expl;
   }else
   {
      result.w = b.w;
      result.color_expl = b.color_expl;
   }
   return result;
}

DotExpl smooth_colored_union(DotExpl a, DotExpl b, float k)
{
   DotExpl result;
   const float h = clamp(0.5 + 0.5 * (b.w - a.w) / k, 0.0, 1.0);
   result.w = lerp(b.w, a.w, h) - k * h * (1.0 - h);
   result.color_expl = lerp(b.color_expl, a.color_expl, h);
   return result;
}

DotExpl smooth_colored_intersection(DotExpl a, DotExpl b, float k)
{
   DotExpl result;
   const float h = clamp(0.5 - 0.5 * (b.w - a.w) / k, 0.0, 1.0);
   result.w = lerp(b.w, a.w, h) + k * h * (1.0 - h);
   result.color_expl = lerp(b.color_expl, a.color_expl, h);
   return result;
}

DotExpl smooth_colored_subtraction(DotExpl a, DotExpl b, float k)
{
   DotExpl result;
   const float h = clamp(0.5 - 0.5 * (a.w + b.w) / k, 0.0, 1.0);
   result.w = lerp(a.w, -b.w, h) + k * h * (1.0 - h);
   result.color_expl =  lerp(a.color_expl, b.color_expl, h);
   return result;
}

float smooth_union(float a, float b, float k)
{
   const float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
   return lerp(b, a, h) - k * h * (1.0 - h);
}

float smooth_subtraction(float a, float b, float k)
{
   const float h = clamp(0.5 - 0.5 * (a + b) / k, 0.0, 1.0);
   return lerp(a, -b, h) + k * h * (1.0 - h);
}

float smooth_intersection(float a, float b, float k)
{
   const float h = clamp(0.5 - 0.5 * (b - a) / k, 0.0, 1.0);
   return lerp(b, a, h) + k * h * (1.0 - h);
}

DotExpl disform(DotExpl dot,float r)
{
   dot.w -= r;
   return dot;
}

float disform(float w,float r)
{
   return w-r;
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

float3 sdf_pos_repeat_3D(float3 p, float3 size)
{
   return p-round(p/size)*size;
}

float3 sdf_pos_repeat_2D(float3 p, float2 size)
{
   return p-round(p/float3(size,1))*float3(size,1);
}

float island_transform(float x)
{
   if(x>=0)return 1;
   return 1/(-0.01*x+1);
}

//Island-like scaling
float3 sdf_pos_islandlize(float3 p,float3 origin,float h)
{
   if(p.y>h)
      return p;
   const float sliceScale = island_transform(p.y-h);
   return float3(p.x-origin.x,0,p.z-origin.z)/sliceScale+float3(origin.x,p.y,origin.z);
}

DotExpl sdf_postIslandlize(float3 p,DotExpl dot,float h,float isoLevel)
{
   if(p.y>h)
      return dot;
   dot.w  = (dot.w-isoLevel)*island_transform(p.y-h) + isoLevel;
   return dot;
}

#endif