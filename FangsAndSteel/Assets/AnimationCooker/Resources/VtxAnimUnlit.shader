// This shader is used to playback prebaked vertex animation information
// 
// Linear interpolation of positions is done based on time.
// 
// Originally there was support for (float/ARGBHalf), but I removed it because it caused several
// branches. Also, the other mode is far more memory efficient, is better supported on mobile devices, 
// and seems to be quite accurate. So it didn't make sense to keep supporting the float mode.
// The current texture format uses ARGB32
//     It stores positions as R11G10B11 (aka X11Y10Z11). 
//     Positional accuracy is 10 or 11 bits instead of 16 bits in ARGBHalf.
// 
// Some parameters are setup for access via DOTS instancing properties.
// 
// See VtxAnimFunctions.hlsl for information about unwrapping pixel coordinates.
//===============================================================================================

//================================= HEADER CONTENTS =============================================
// The header occupies row 0 (which in GL is the bottom row)
// See FillHeaderLine() in AnimationCookerUtils.cs for more info.
// [pixel 0] (4x8bit) misc bytes
//    1 byte Version number - gets incremented whenever the shader format changes
//    1 byte Frame rate - the sample rate used for all animation clips (255fps max)
//    1 byte unused
//    1 byte width-pow2 - the power of two associated with the width: log2(width). ex: 256 --> 8, 512 --> 9.
// [pixel 1] (2x16bit) bounding-box min/max used when scaling vertex positions to R11G10B11
//    1 short - the smallest possible vertex position
//    1 short - the largest possible vertex position
// [pixel 2] (1x32bit) mesh vertex count
//    1 unsigned int - the number of vertexes that the mesh posseses
// [pixel 3..clip-count] (2x16bit) - each animation clip gets a pixel that specifies the begin/end frame index
//  The frame index is cumulative. so if clip0 has 2f, clip1 has 4f, and clip3 has 3f, 
//  then the frame indexes are: [0,1,2,3,4,5,6,7,8]. clip1 --> begin: 2, end --> 5, count: begin - end + 1.
//  Since min width is 128, an error is possible if there are > 125 animation clips and a width of 128.
//    1 short - the beginning frame index
//    1 short - the ending frame index
// [remaining pixels] uninitialized to zero.
//================================================================================================

Shader "AnimationCooker/VtxAnimUnlit"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo texture [_BaseMap]", 2D) = "white"{}
        _PosMap("Position texture [_PosMap]", 2D) = "black"{}
        _NmlMap("Normal texture [_NmlMap]", 2D) = "black"{}
        _TanMap("Tangent texture [_TanMap]", 2D) = "black"{}
        _CurTime("Show animation at specified time [_CurTime]", Float) = 0
        _ClipIdx("Current clip index [_ClipIdx]", Float) = 0
        _SpeedInst("Playback speed per instance (0..n) [_SpeedInst]", Float) = 1
        _SpeedMat("Playback speed per material (0..n) [_SpeedMat]", Float) = 1
        _BaseColor("Tint color [_BaseColor]", Color) = (1,1,1,1)
        [Toggle(ALPHA_CLIP)] _AlphaClip("Alpha clipping [_AlphaClip]", Float) = 0
        _AlphaCutoff("Alpha clip cutoff [_AlphaCutoff]", Range(0.0, 1.0)) = 0.5

        [HideInInspector] _Color("Base Color [_Color]", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100 Cull Off

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "VtxAnimFunctions.hlsl"
            #pragma target 4.5
            #pragma multi_compile _ DOTS_INSTANCING_ON
            //#pragma enable_d3d11_debug_symbols

            CBUFFER_START(UnityPerMaterial)
                float4 _PosMap_TexelSize; // built in variable {texturename}_TexelSize
                float4 _NmlMap_TexelSize; // built in variable {texturename}_TexelSize
                float4 _TanMap_TexelSize; // built in variable {texturename}_TexelSize
                float _SpeedInst;
                float _SpeedMat;
                half4 _Color;
                half4 _BaseColor;
                float _AlphaClip;
                float _ClipIdx;
                float _CurTime;
                float4 _BaseMap_ST;
                float _AlphaCutoff;
            CBUFFER_END

			#ifdef UNITY_DOTS_INSTANCING_ENABLED
                // DOTS instancing definitions
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                    UNITY_DOTS_INSTANCED_PROP(float4, _Color)
                    UNITY_DOTS_INSTANCED_PROP(float, _CurTime)
                    UNITY_DOTS_INSTANCED_PROP(float, _ClipIdx)
                    UNITY_DOTS_INSTANCED_PROP(float, _SpeedInst)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
        
                // DOTS instancing usage macros
                //#define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(float4 , Metadata_BaseColor)
				#define _BaseColor UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _BaseColor)
        
                #define _Color UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _Color)
                #define _SpeedInst UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _SpeedInst)
                #define _CurTime UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _CurTime)
                #define _ClipIdx UNITY_ACCESS_DOTS_INSTANCED_PROP(float, _ClipIdx)
            #endif
        ENDHLSL

        Pass {
            HLSLPROGRAM
            //#pragma enable_d3d11_debug_symbols
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            // I think the ___ below means "precompile" two versions of the shader - one with the define and one without
            #pragma multi_compile ___ ALPHA_CLIP

            struct appdata
            {
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _BaseMap, _PosMap, _NmlMap, _TanMap;// , _NmlTex;

            v2f vert(appdata v, uint vertexId : SV_VertexID)
            {
                // IMPORTANT! It is very important to setup the instance ID this BEFORE using any of the instanced variables!!
                // (thanks jdtech!)
                UNITY_SETUP_INSTANCE_ID(v);

                float3 position = DerivePosition(_PosMap_TexelSize, _PosMap, _SpeedInst, _SpeedMat, _ClipIdx, _CurTime, vertexId);

                // make the return value (output)
                v2f o;
                // transfer the instance ID from the vertex to the fragment function. 
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                // now store the vertex positions and normals and return the output
                o.position = ObjectToClipPos(position);
                o.uv = v.uv;
                return o;
            }

            // this is the fragment function. i don't fully understand it yet.
            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float diffuse = 0.5;
                half4 color = tex2D(_BaseMap, i.uv) * _Color * 1.0;
                //half4 color = tex2D(_BaseMap, i.uv) * float4(1,1,1,1) * 1.0;
                #ifdef ALPHA_CLIP
                    half alpha = color.a * _BaseColor.a;
                    clip(alpha - _AlphaCutoff);
                    return half4(diffuse * color);
                #else
                    return diffuse * color;
                #endif
            }
            ENDHLSL
        } // pass
        //UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    } // subshader
}