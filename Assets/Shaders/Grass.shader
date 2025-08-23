Shader "Custom/Grass"
{
    Properties
    {
        _BaseColor ("Base Colour", Color) = (0, 0.66, 0.73, 1)
        _SpecularColor ("Specular Colour", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        HLSLINCLUDE

        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile _ _SHADOWS_SOFT
        
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Assets/Scripts/ProceduralGenerator/Generator/Compute/Includes/QuaternionEssential.hlsl"
        #include "Assets/Scripts/ProceduralGenerator/Generator/Compute/Noise/FastSnoise.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _SpecularColor;
        CBUFFER_END

        struct InstanceData
        {
            float3 position;
            float height;
            float width;
            float darkness;
            float4 origin_quaternion;
            float4 disturbance_quaternion;
        };


        UNITY_INSTANCING_BUFFER_START(instance_buffer)
            StructuredBuffer<InstanceData> instance_buffer;
        UNITY_INSTANCING_BUFFER_END(instance_buffer)

        float3 grass_mesh_size;

        float4 GetBendedVertex(float4 vertex,float3 normal,float height,float4 origin_quaterion, float4 bend_quaternion,out float3 bended_normal)
        {
            float w = vertex.w;
            vertex /= w;
            float4 final_quaterion = q_slerp(origin_quaterion,bend_quaternion,vertex.y/height);
            bended_normal = normalize(rotate_vector(normal,final_quaterion));
            return w*float4(rotate_vector(vertex,final_quaterion),1);
        }
        
        ENDHLSL

        Pass
        {
            Tags
            {
                "LightMode"="UniversalForward"
            }
            
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #pragma instancing_options procedural:setup
            

            // Structs
            struct a2v
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normal : NORMAL;
                float darkness : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float4 positionOS : TEXCOORD2;
            };

            

            // Vertex Shader
            v2f UnlitPassVertex(a2v IN, uint id : SV_InstanceID)
            {
                v2f OUT;
                float4 obj_pos = IN.positionOS;
                InstanceData instance = instance_buffer[id];
                
                obj_pos *= float4(instance.width, instance.height, 1,1);
                float3 bended_normal;
                obj_pos = GetBendedVertex(obj_pos,TransformObjectToWorld(IN.normal),grass_mesh_size.y * instance.height,
                    instance.origin_quaternion,instance.disturbance_quaternion,bended_normal);
                const VertexPositionInputs positionInputs = GetVertexPositionInputs(obj_pos);
                const float3 position_ws = instance.position + positionInputs.positionWS;
                OUT.positionCS = TransformWorldToHClip(position_ws);
                OUT.darkness = instance.darkness;
                OUT.normal = bended_normal;
                OUT.positionWS = position_ws;
                OUT.positionOS = IN.positionOS;
                return OUT;
            }

            half3 ApplySingleDirectLight(Light light, half3 N, half3 V, half3 albedo, half positionOSY)
            {
                half3 H = normalize(light.direction + V);
                half directDiffuse = dot(N, light.direction) * 0.5 + 0.5; //half lambert, to fake grass SSS
                float directSpecular = saturate(dot(N,H));
                pow(directSpecular,8);
                directSpecular *= directSpecular;
                directSpecular *= 0.1 * positionOSY;//only apply directSpecular to grass's top area, to simulate grass AO
                half3 lighting = light.color * (light.shadowAttenuation * light.distanceAttenuation);
                half3 result = (albedo * directDiffuse + _SpecularColor * directSpecular)* lighting;
                return result; 
            }

            // Fragment Shader
            half4 UnlitPassFragment(v2f IN) : SV_Target
            {
                Light mainLight;
            #if _MAIN_LIGHT_SHADOWS
                mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
            #else
                mainLight = GetMainLight();
            #endif
                half3 albedo = _BaseColor * IN.darkness;
                half3 lightingResult = SampleSH(0) * albedo;
                half3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                lightingResult += ApplySingleDirectLight(mainLight,IN.normal,viewDir,albedo,IN.positionOS.y);
                #if _ADDITIONAL_LIGHTS
                int additionalLightsCount = GetAdditionalLightsCount();
                for (int i = 0; i < additionalLightsCount; ++i)
                {
                    Light light = GetAdditionalLight(i, IN.positionWS);
                    lightingResult += ApplySingleDirectLight(light, IN.normal,viewDir,albedo,IN.positionOS.y);
                }
                #endif
                 //fog
                float fogFactor = ComputeFogFactor(IN.positionCS.z);
                lightingResult = MixFog(lightingResult, fogFactor);
                return half4(lightingResult,1);
            }
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode"="ShadowCaster"
            }
            
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex UnlitPassVertexShadow
            #pragma fragment UnlitPassFragmentShadow
            #pragma instancing_options procedural:setup
            

            // Structs
            struct a2v
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            };

            // Vertex Shader
            v2f UnlitPassVertexShadow(a2v IN, uint id : SV_InstanceID)
            {
                v2f OUT;
                float4 objPos = IN.positionOS;
                InstanceData instance = instance_buffer[id];
                objPos *= float4(instance.width, instance.height, 1,1);
                float3 normalWS = TransformObjectToWorldNormal(IN.normal);
                float3 bendedNormal;
                objPos = GetBendedVertex(objPos,normalWS,grass_mesh_size.y * instance.height,
                    instance.origin_quaternion,instance.disturbance_quaternion,bendedNormal);
                const VertexPositionInputs positionInputs = GetVertexPositionInputs(objPos);
                const float3 position_ws = instance.position + positionInputs.positionWS;
                float4 shadowPositionCS=TransformWorldToHClip(ApplyShadowBias(position_ws,bendedNormal,GetMainLight().direction));
      		#if UNITY_REVERSED_Z
      		    shadowPositionCS.z=min(shadowPositionCS.z,shadowPositionCS.w*UNITY_NEAR_CLIP_VALUE);
      		#else
      		    shadowPositionCS.z=max(shadowPositionCS.z,shadowPositionCS.w*UNITY_NEAR_CLIP_VALUE);
      		#endif
                
                OUT.positionCS = shadowPositionCS;
                return OUT;
            }

            // Fragment Shader
            half4 UnlitPassFragmentShadow(v2f IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}