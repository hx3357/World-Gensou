Shader "Custom/Grass"
{
    Properties
    {
        _BaseColor ("Example Colour", Color) = (0, 0.66, 0.73, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
        CBUFFER_END

        struct InstanceData
        {
            float3 position;
            float height;
            float width;
            float darkness;
            float angle_dir;
            float bend;
        };

        float3 rotate_xz(float3 v, float angle)
        {
            float s = sin(angle);
            float c = cos(angle);
            return float3(v.x * c - v.z * s, v.y, v.x * s + v.z * c);
        }

        
        UNITY_INSTANCING_BUFFER_START(instance_buffer)
        StructuredBuffer<InstanceData> instance_buffer;
        UNITY_INSTANCING_BUFFER_END(instance_buffer)
        
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #pragma instancing_options procedural:setup

            // Structs
            struct a2v
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            };

            // Vertex Shader
            v2f UnlitPassVertex(a2v IN, uint id : SV_InstanceID)
            {
                v2f OUT;
                float3 obj_pos  = IN.positionOS.xyz;
                obj_pos *= 0.3f;
                obj_pos = rotate_xz(obj_pos, instance_buffer[id].position.x + instance_buffer[id].position.y + instance_buffer[id].position.z);
                const VertexPositionInputs positionInputs = GetVertexPositionInputs(obj_pos);
                const float3 position = instance_buffer[id].position + positionInputs.positionWS;
                OUT.positionCS = TransformWorldToHClip(position);
                return OUT;
            }

            // Fragment Shader
            half4 UnlitPassFragment(v2f IN) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}