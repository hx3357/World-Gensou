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
        #include "Assets/Scripts/ProceduralGenerator/Generator/Compute/Includes/QuaternionEssential.hlsl"
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
            float4 origin_quaternion;
            float4 disturbance_quaternion;
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

            float4 GetBendedVertex(float4 vertex,float height,float4 origin_quaterion, float4 bend_quaternion)
            {
                float4 final_quaterion = q_slerp(origin_quaterion,bend_quaternion,vertex.y/height);
                return qmul(final_quaterion,vertex);
            }

            // Vertex Shader
            v2f UnlitPassVertex(a2v IN, uint id : SV_InstanceID)
            {
                v2f OUT;
                float4 obj_pos = IN.positionOS;
                InstanceData instance = instance_buffer[id];
                
                obj_pos *= float4(instance.width, instance.height, 1,1);
                obj_pos = GetBendedVertex(obj_pos,grass_mesh_size.y * instance.height,
                    instance.origin_quaternion,instance.disturbance_quaternion);
                
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