Shader"Unlit/FogOfWarShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "ForceNoShadowCasting" = "True" "PreviewType" = "Plane"}

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 vertexWS : TEXCOORD1;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
            };

            #ifdef DOTS_INSTANCING_ON
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _Color)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
                #define _Color UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _Color)
            #endif

            extern StructuredBuffer<int> _VisionMap;
            CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            float4 _MainTex_ST;
int _curTeam;
            CBUFFER_END

            //extern StructuredBuffer<int> _VisionMap;
            //sampler2D _MainTex;
            //float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
    
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                //o.vertex = GetVertexPositionInputs(v.vertex).positionCS;
                o.vertexWS = TransformObjectToWorld(v.vertex);
                o.vertex = TransformWorldToHClip(o.vertexWS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4  frag (v2f i) : SV_Target
            {
                float x = (i.vertexWS.x) + 500;
                float y = (i.vertexWS.z) + 500;
                int visInfo = _VisionMap[(int) (floor(x / 2) + floor(y / 2) * 500)];
                //point = math.floor(localtoworld.Position.xz + curpointline + VIS_MAP_SIZE);
                //int idx = (int) (point.x / ORIG_TO_VIS_MAP_RATIO + math.floor(point.y / ORIG_TO_VIS_MAP_RATIO) * VIS_MAP_SIZE);

                //int idx = (int) (floor(x / 2) + floor(y / 2) * 500);
                //return float4(1, 1, 1, 
                //(_VisionMap[idx] & _curTeam == 0) * 0.3 + 
                //(_VisionMap[(idx + 1) % 250000] & _curTeam == 0) * 0.05 +
                //(_VisionMap[(idx - 1) % 250000] & _curTeam == 0) * 0.05 +
                //(_VisionMap[(idx + 500) % 250000] & _curTeam == 0) * 0.05 + 
                //(_VisionMap[(idx - 500) % 250000] & _curTeam == 0) * 0.05);
    
                if (visInfo & _curTeam != 0)
                    return float4(0, 1, 0, 0.3);
                else
                    return float4(1, 0, 0, 0.6);
                }
            ENDHLSL
        }
    }
}
