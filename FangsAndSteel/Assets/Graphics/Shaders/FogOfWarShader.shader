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
            int _curTeam;
            CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            float4 _MainTex_ST;
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
                //int visInfo = _VisionMap[(int) (floor(x / 2) + floor(y / 2) * 500)];
                //point = math.floor(localtoworld.Position.xz + curpointline + VIS_MAP_SIZE);
                //int idx = (int) (point.x / ORIG_TO_VIS_MAP_RATIO + math.floor(point.y / ORIG_TO_VIS_MAP_RATIO) * VIS_MAP_SIZE);
            
                //int idx = (int) (floor(x / 2) + floor(y / 2) * 500);
    
    //float centerX = (idx * 2) % 1000;
    //float centerY = (idx * 2 - centerX) /500;
    //float2 centerPoint = float2(centerX, centerY);
    //int x = (index * ORIG_TO_VIS_MAP_RATIO) % ORIG_MAP_SIZE;
    //int y = (index * ORIG_TO_VIS_MAP_RATIO - x) / VIS_MAP_SIZE;
    
                float step_w = 1.5f;
                float step_h = 1.5f;
                float2 offset[25] =
                {
                    float2(-step_w * 2.0, -step_h * 2.0), float2(-step_w, -step_h * 2.0), float2(0.0, -step_h * 2.0), float2(step_w, -step_h * 2.0), float2(step_w * 2.0, -step_h * 2.0),
                                float2(-step_w * 2.0, -step_h), float2(-step_w, -step_h), float2(0.0, -step_h), float2(step_w, -step_h), float2(step_w * 2.0, -step_h),
                                float2(-step_w * 2.0, 0.0), float2(-step_w, 0.0), float2(0.0, 0.0), float2(step_w, 0.0), float2(step_w * 2.0, 0.0),
                                float2(-step_w * 2.0, step_h), float2(-step_w, step_h), float2(0.0, step_h), float2(step_w, step_h), float2(step_w * 2.0, step_h),
                                float2(-step_w * 2.0, step_h * 2.0), float2(-step_w, step_h * 2.0), float2(0.0, step_h * 2.0), float2(step_w, step_h * 20), float2(step_w * 2.0, step_h * 2.0)
                };

                float kernel[25] =
                {

                    0.003765, 0.015019, 0.023792, 0.015019, 0.003765,
                                0.015019, 0.059912, 0.094907, 0.059912, 0.015019,
                                0.023792, 0.094907, 0.150342, 0.094907, 0.023792,
                                0.015019, 0.059912, 0.094907, 0.059912, 0.015019,
                                0.003765, 0.015019, 0.023792, 0.015019, 0.003765
                };

                float4 sum = float4(0.0, 0.0, 0.0, 0.0);
                
                
                for (int j = 0; j < 25; j++)
                {
                    int idx = (int) (floor((x + offset[j].x) / 2) + floor((y + offset[j].y) / 2) * 500);
                    float4 visInfoCol = float4(0, 0, 0, (abs((_VisionMap[idx] & _curTeam) - _curTeam)) * 0.6);
                    sum += visInfoCol * kernel[j];
                }

                return sum;
    
                //return float4(0, 0, 0, 
                //(abs((_VisionMap[idx] & _curTeam) - _curTeam)) * 0.6 * distance(centerPoint, float2(x, y)) / 2 +
                //(abs((_VisionMap[(idx + 1) % 250000] & _curTeam) - _curTeam)) * 0.05 +
                //(abs((_VisionMap[(idx - 1) % 250000] & _curTeam) - _curTeam)) * 0.05 +
                //(abs((_VisionMap[(idx + 500) % 250000] & _curTeam) - _curTeam)) * 0.05 + 
                //(abs((_VisionMap[(idx - 500) % 250000] & _curTeam) - _curTeam)) * 0.05);
                //
                //if (visInfo & _curTeam != 0)
                //    return float4(0, 1, 0, 0.3);
                //else
                //    return float4(1, 0, 0, 0.6);
                }
            ENDHLSL
        }
    }
}
