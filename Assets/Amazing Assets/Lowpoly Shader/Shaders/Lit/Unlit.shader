Shader "Amazing Assets/Lowpoly Shader/Unlit"
{
    Properties
    { 
//[HideInInspector][CurvedWorldBendSettings] _CurvedWorldBendSettings("0|1|1", Vector) = (0, 0, 0, 0)


        [HideInInspector][MaterialEnum(Front,2,Back,1,Both,0)] _Lowpoly_Cull("Face Cull", Int) = 0
        [HideInInspector][KeywordEnum(None, Lowpoly Color Alpha, Diffuse Texture Alpha)] _Lowpoly_AlphaTest ("Aplha Test", Float) = 0
        [HideInInspector][Enum(Lowpoly Alpha, 0, Diffuse Alpha, 1)] _Lowpoly_TransparentFade ("Transparent Fade", Float) = 0

        [HideInInspector] _Color ("Color", Color) = (1,1,1,1)
        [HideInInspector][LowpolyShaderToggleFloat] _Lowpoly_UseVertexColor("Use Vertex Color", int) = 0

        [HideInInspector] _MainTex ("MainTex", 2D) = "white" {}
        [HideInInspector][LowpolyShaderUVScroll] _MainTex_Scroll ("Scroll", Vector) = (0, 0, 0, 0)
        [HideInInspector][KeywordEnum(None, Additive, Multiply, Alpha Blend)] _Lowpoly_SecondaryTextureBlendMode ("Blend Mode", Float) = 0
        [HideInInspector] _Lowpoly_SecondaryTex ("SecondaryTex", 2D) = "white" {}
        [HideInInspector] _Lowpoly_SecondaryColor ("Secondary Color", Color) = (1, 1, 1, 1)
        [HideInInspector][LowpolyShaderUVScroll] _Lowpoly_SecondaryTex_Scroll ("Scroll", Vector) = (0, 0, 0, 0)
        [HideInInspector][LowpolyShaderToggleFloat] _Lowpoly_SecondaryTex_InvertAlpha("SecondaryTex Invert Alpha", int) = 0
        [HideInInspector] _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
         
        [HideInInspector] [KeywordEnum(None, Parametric, One Texture, Two Textures)] _Lowpoly_DisplaceMode ("Displace Mode", Float) = 0
        [HideInInspector][LowpolyShaderToggleFloat] _Lowpoly_DisplaceScriptSynchronize("Displace Script Synchronize", Float) = 0
        [HideInInspector] _Lowpoly_DisplaceDirection("Displace Direction", Range(0, 360)) = 45
		[HideInInspector] _Lowpoly_DisplaceSpeed("Displace Speed", Float) = 20
		[HideInInspector] _Lowpoly_DisplaceAmplitude("Displace Amplitude", Float) = 0.5
		[HideInInspector] _Lowpoly_DisplaceFrequency("Displace Frequency", Float) = 0.2
		[HideInInspector] _Lowpoly_DisplaceNoiseCoef("Displace Noise Coef", Float) = -0.5    

         
        [HideInInspector] _Lowpoly_DisplaceMap ("Displace Map", 2D) = "black" {}
        [HideInInspector][LowpolyShaderUVScroll] _Lowpoly_DisplaceMap_Scroll ("Scroll", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(Red, 0, Green, 1, Blue, 2, Alpha, 3)] _Lowpoly_DisplaceMapChannel ("Displace Channel", int) = 0
        [HideInInspector] _Lowpoly_DisplaceMapStrength("Displace Strength", float) = 1
        [HideInInspector] _Lowpoly_DisplaceSecondaryMap ("Displace Secondary Map", 2D) = "black" {}
        [HideInInspector][LowpolyShaderUVScroll] _Lowpoly_DisplaceSecondaryMap_Scroll ("Scroll", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(Red, 0, Green, 1, Blue, 2, Alpha, 3)] _Lowpoly_DisplaceSecondaryMapChannel ("Displace Channel", int) = 0
        [HideInInspector] _Lowpoly_DisplaceSecondaryMapStrength("Displace Strength", float) = 1
        [HideInInspector][Enum(Additive, 0, Multiply, 1)] _Lowpoly_DisplaceMapsBlendMode ("Blend Mode", Float) = 1

        [HideInInspector][KeywordEnum(Average Color, Middle UV, First Index)] _Lowpoly_SampleType("Sample Type", Float) = 0
        [HideInInspector][Toggle(_LOWPOLY_FLAT_NORMALS)] _Lowpoly_FlatNormals("Flat Normals", Float) = 0
        [HideInInspector][Toggle(_LOWPOLY_FLAT_LIGHTMAPS)] _Lowpoly_FlatLightMaps("Flat Lightmaps", Float) = 0


        [HideInInspector] _Lowpoly_DiffuseColor ("Color", Color) = (1,1,1,1)
        [HideInInspector] _Glossiness ("Smoothness", Range(0,1)) = 0.5
        [HideInInspector] _Metallic ("Metallic", Range(0,1)) = 0.0
        [HideInInspector][Enum(None, 0, Lowpoly Color Alpha, 1, Diffuse Texture Alpha, 2)] _Lowpoly_SmoothnessSource ("Smoothness Source", int) = 0

        [HideInInspector][KeywordEnum(None, Additive, Multiply, Alpha Blend, Color Burn, Linear Burn, Screen, Overlay, Hard Light)] _Lowpoly_DiffuseBlendMode ("Diffuse Blend Mode", Float) = 0
        [HideInInspector] _Lowpoly_DiffuseBlendStrength ("Blend Strength", Range(0, 1)) = 1
        [HideInInspector][LowpolyShaderToggleFloat] _Lowpoly_DiffuseBlendAlphaPremultiply ("Alpha Premultiply", float) = 0

        [HideInInspector] _Lowpoly_DiffuseMap("Diffuse Map", 2D) = "black"{}
        [HideInInspector][LowpolyShaderUVScroll] _Lowpoly_DiffuseMap_Scroll ("Scroll", Vector) = (0, 0, 0, 0)

        [HideInInspector] _Lowpoly_BumpMap("Bump Map", 2D) = "bump"{}
        [HideInInspector][LowpolyShaderUVScroll] _Lowpoly_BumpMap_Scroll ("Scroll", Vector) = (0, 0, 0, 0)
        [HideInInspector] _Lowpoly_BumpMapStrength("Strength", float) = 1

        
        //Wireframe
        [HideInInspector] _Lowpoly_WireframeThickness("Thickness", Range(0, 1)) = 0.01
		[HideInInspector] _Lowpoly_WireframeSmoothness("Smoothness", Range(0, 1)) = 0	
        [HideInInspector] _Lowpoly_WireframeDiameter("Diameter", Range(0, 1)) = 1
        [HideInInspector][Toggle(_LOWPOLY_WIREFRAME_NORMALIZE_EDGES)] _Lowpoly_WireframeNormalizeEdges ("Normalize Edges", Float) = 0
        [HideInInspector]_Lowpoly_WireframeColor("Color", Color) = (1, 0, 0, 1)
        [HideInInspector]_Lowpoly_WireframeColorEmission("Emission", float) = 0
        [HideInInspector][KeywordEnum(None, Additive, Multiply)] _Lowpoly_WireframeMode("Mode", int) = 0  
        [HideInInspector][Toggle(_LOWPOLY_WIREFRAME_TRY_QUADS)] _Lowpoly_WireframeTryQuads("Try Quads", Float) = 0
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Cull[_Lowpoly_Cull]  



        	

	// ---- forward rendering base pass:
	Pass 
    {

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma geometry geom
#pragma fragment frag_surf

#pragma multi_compile_fog
#include "UnityCG.cginc"



struct v2f_surf 
{
  UNITY_POSITION(pos);
  
  float4 texcoord : TEXCOORD0; // _MainTex
  fixed4 color : COLOR0;
  float3 worldPos : TEXCOORD1;

  UNITY_FOG_COORDS(2)

  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO

  //Lowpoly
  float3 wireframe : TEXCOORD3;
};




//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"

#pragma shader_feature_local _ _LOWPOLY_ALPHATEST_LOWPOLY_COLOR_ALPHA _LOWPOLY_ALPHATEST_DIFFUSE_TEXTURE_ALPHA

#pragma shader_feature_local _LOWPOLY_SAMPLETYPE_AVERAGE_COLOR _LOWPOLY_SAMPLETYPE_MIDDLE_UV _LOWPOLY_SAMPLETYPE_FIRST_INDEX
#pragma shader_feature_local _ _LOWPOLY_SECONDARYTEXTUREBLENDMODE_ADDITIVE _LOWPOLY_SECONDARYTEXTUREBLENDMODE_MULTIPLY _LOWPOLY_SECONDARYTEXTUREBLENDMODE_ALPHA_BLEND
#pragma shader_feature_local _ _LOWPOLY_DISPLACEMODE_PARAMETRIC _LOWPOLY_DISPLACEMODE_ONE_TEXTURE _LOWPOLY_DISPLACEMODE_TWO_TEXTURES
#pragma shader_feature_local _ _LOWPOLY_WIREFRAMEMODE_ADDITIVE _LOWPOLY_WIREFRAMEMODE_MULTIPLY
#pragma shader_feature_local _LOWPOLY_WIREFRAME_TRY_QUADS
#pragma shader_feature_local _LOWPOLY_WIREFRAME_NORMALIZE_EDGES
#pragma shader_feature_local _ _LOWPOLY_DIFFUSEBLENDMODE_ADDITIVE _LOWPOLY_DIFFUSEBLENDMODE_MULTIPLY _LOWPOLY_DIFFUSEBLENDMODE_ALPHA_BLEND _LOWPOLY_DIFFUSEBLENDMODE_COLOR_BURN _LOWPOLY_DIFFUSEBLENDMODE_LINEAR_BURN _LOWPOLY_DIFFUSEBLENDMODE_SCREEN _LOWPOLY_DIFFUSEBLENDMODE_OVERLAY _LOWPOLY_DIFFUSEBLENDMODE_HARD_LIGHT


#define LOWPOLY_GEOMETRY_READ_WORLD_POSITION_WORLD_SPACE
#define _LOWPOLY_UNLIT
#include "Lit.cginc"


// vertex shader
v2f_surf vert_surf (appdata_full v) {

  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
   

  //Lowpoly
  v.vertex = SetupLowpolyVertexDisplace(v);

  //Curved World
  #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
      #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
          CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v.vertex, v.normal, v.tangent)
      #else
          CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
      #endif
  #endif


  o.pos = UnityObjectToClipPos(v.vertex);
  o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  o.texcoord = v.texcoord;
  o.color = v.color;

  UNITY_TRANSFER_FOG(o,o.pos);


  //Lowpoly
  o.color = SetupLowpolyColor(v.texcoord.xy, v.color);


  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {

  float3 albedo;
  float alpha;
  float3 emission;

  // call surface function
  surf (IN, albedo, alpha, emission);

  fixed4 c = 0;

  c.rgb = albedo;
  c.rgb += emission;
  c.a = alpha;
 

  // apply fog
  UNITY_APPLY_FOG(IN.fogCoord, c);
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


ENDCG

}



// ---- shadow caster pass:
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		ZWrite On ZTest LEqual

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma geometry geom
#pragma fragment frag_surf
#pragma multi_compile_instancing
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_shadowcaster
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

       

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float4 texcoord : TEXCOORD0; // _MainTex
  float3 worldPos : TEXCOORD1;
  fixed4 color : COLOR0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
}; 


//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"

#pragma shader_feature_local _ _LOWPOLY_ALPHATEST_LOWPOLY_COLOR_ALPHA _LOWPOLY_ALPHATEST_DIFFUSE_TEXTURE_ALPHA

#pragma shader_feature_local _LOWPOLY_SAMPLETYPE_AVERAGE_COLOR _LOWPOLY_SAMPLETYPE_MIDDLE_UV _LOWPOLY_SAMPLETYPE_FIRST_INDEX
#pragma shader_feature_local _ _LOWPOLY_SECONDARYTEXTUREBLENDMODE_ADDITIVE _LOWPOLY_SECONDARYTEXTUREBLENDMODE_MULTIPLY _LOWPOLY_SECONDARYTEXTUREBLENDMODE_ALPHA_BLEND
#pragma shader_feature_local _ _LOWPOLY_DISPLACEMODE_PARAMETRIC _LOWPOLY_DISPLACEMODE_ONE_TEXTURE _LOWPOLY_DISPLACEMODE_TWO_TEXTURES
#pragma shader_feature_local _ _LOWPOLY_WIREFRAMEMODE_ADDITIVE _LOWPOLY_WIREFRAMEMODE_MULTIPLY
#pragma shader_feature_local _LOWPOLY_WIREFRAME_TRY_QUADS
#pragma shader_feature_local _LOWPOLY_WIREFRAME_NORMALIZE_EDGES
#pragma shader_feature_local _ _LOWPOLY_DIFFUSEBLENDMODE_ADDITIVE _LOWPOLY_DIFFUSEBLENDMODE_MULTIPLY _LOWPOLY_DIFFUSEBLENDMODE_ALPHA_BLEND _LOWPOLY_DIFFUSEBLENDMODE_COLOR_BURN _LOWPOLY_DIFFUSEBLENDMODE_LINEAR_BURN _LOWPOLY_DIFFUSEBLENDMODE_SCREEN _LOWPOLY_DIFFUSEBLENDMODE_OVERLAY _LOWPOLY_DIFFUSEBLENDMODE_HARD_LIGHT


#include "Lit.cginc"


// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


  //Lowpoly
  v.vertex = SetupLowpolyVertexDisplace(v);

  //Curved World
  #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
      #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
          CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v.vertex, v.normal, v.tangent)
      #else
          CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
      #endif
  #endif


  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.texcoord = v.texcoord;
  o.worldPos.xyz = worldPos;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)


  
  //Lowpoly
  o.color = SetupLowpolyColor(v.texcoord.xy, v.color);


  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);



  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;

  // call surface function
  surf (IN, o);

  SHADOW_CASTER_FRAGMENT(IN)
}


ENDCG

}
    }
    
    CustomEditor "AmazingAssets.LowpolyShader.LitShaderGUI"
}
