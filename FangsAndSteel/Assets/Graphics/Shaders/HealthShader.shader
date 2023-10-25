Shader"Unlit/HealthShader"
{
    Properties
    {
        _Health ("Health", Range(0, 1)) = 0
        [MainTexture]_Texture ("Texture", 2D) = "white" {}
        _LowColor ("Low Health Color", Color) = (1, 0, 0, 1)
        _HighColor ("High Health Color", Color) = (0, 1, 0, 1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "ForceNoShadowCasting" = "True" "PreviewType" = "Plane" }

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
        

            #pragma multi_compile _ DOTS_INSTANCING_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };

            struct Interpolator
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
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

            TEXTURE2D(_Texture); SAMPLER(sampler_Texture);
            CBUFFER_START(UnityPerMaterial)
                float4 _Texture_ST;
                half _Health;
            CBUFFER_END

            Interpolator vert (VertexInput input)
            {
                Interpolator output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                
                output.positionCS = GetVertexPositionInputs(input.positionOS).positionCS;
                input.uv.x += 0.5 - _Health;
                output.uv = TRANSFORM_TEX(input.uv, _Texture);
                return output;
            }

            float4 frag (Interpolator input) : SV_Target
            {
                float4 textureColor = SAMPLE_TEXTURE2D(_Texture, sampler_Texture, input.uv);
                return textureColor;
            }
            ENDHLSL
        }
    }
}
