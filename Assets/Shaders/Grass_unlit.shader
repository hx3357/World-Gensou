Shader "Unlit/Grass_unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options

            #include "UnityCG.cginc"

            struct InstanceData
            {
                float3 position;
                float height;
                float width;
                float darkness;
                float angle_dir;
                float bend;
            };

            StructuredBuffer<InstanceData> instance_buffer;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert(appdata v, uint id : SV_InstanceID)
            {
                v2f o;
                const float4 world_pos = mul(unity_ObjectToWorld, v.vertex) + float4(instance_buffer[id].position, 0);
                o.vertex = UnityWorldToClipPos(world_pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}