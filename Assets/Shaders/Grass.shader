Shader "Custom/Grass"
{
    Properties
    {
        _BaseColor ("Base Colour", Color) = (0, 0.66, 0.73, 1)
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

        #include "Assets/Scripts/ProceduralGenerator/Generator/Compute/Noise/FastSnoise.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
        CBUFFER_END

        struct InstanceData
        {
            float3 position;
            float height;
            float width;
            float darkness;
            float angle_xz;
            float bend_xz;
            float bend_y;
        };


        UNITY_INSTANCING_BUFFER_START(instance_buffer)
            StructuredBuffer<InstanceData> instance_buffer;
        UNITY_INSTANCING_BUFFER_END(instance_buffer)

        float3 grass_mesh_size;
        
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
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
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float darkness : TEXCOORD0;
            };

            float3 rotate_xz(float3 v, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float3(v.x * c - v.z * s, v.y, v.x * s + v.z * c);
            }

            float3 rotate_yz(float3 v, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float3(v.x, v.y * c - v.z * s, v.y * s + v.z * c);
            }

            float3 GetBendedVertex(float3 vertex, float bend,float angle_xz,float height)
            {
                float3 bended_vertex = rotate_yz(vertex, lerp(0, bend, vertex.y / height));
                bended_vertex = rotate_xz(bended_vertex,lerp(0, angle_xz, vertex.y / height));
                return bended_vertex;
            }

            // Vertex Shader
            v2f UnlitPassVertex(a2v IN, uint id : SV_InstanceID)
            {
                v2f OUT;
                float3 obj_pos = IN.positionOS.xyz;
                InstanceData instance = instance_buffer[id];
                obj_pos *= float3(instance.width, instance.height, 1);
                obj_pos = rotate_xz(obj_pos, instance.angle_xz);
                obj_pos = GetBendedVertex(obj_pos, instance.bend_y,instance.bend_xz,
                    grass_mesh_size.y * instance.height);
                const VertexPositionInputs positionInputs = GetVertexPositionInputs(obj_pos);
                const float3 position = instance.position + positionInputs.positionWS;
                OUT.positionCS = TransformWorldToHClip(position);
                OUT.darkness = instance.darkness;
                return OUT;
            }

            // Fragment Shader
            half4 UnlitPassFragment(v2f IN) : SV_Target
            {
                return _BaseColor * IN.darkness;
            }
            ENDHLSL
        }
    }
}