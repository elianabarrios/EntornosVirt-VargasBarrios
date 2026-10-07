Shader "Portal/RoomTextured"
{
    Properties
    {
        _BaseMap ("Textura", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comp", Float) = 8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Cull Off
            Stencil { Ref 1 Comp [_StencilComp] }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
            CBUFFER_END

            struct A { float4 p : POSITION; float2 uv : TEXCOORD2; };
            struct V { float4 p : SV_POSITION; float2 uv : TEXCOORD0; };

            V vert(A i)
            {
                V o;
                o.p = TransformObjectToHClip(i.p.xyz);
                o.uv = TRANSFORM_TEX(float2(i.uv.x, 1 - i.uv.y), _BaseMap);
                return o;
            }

            half4 frag(V i) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
            }
            ENDHLSL
        }
    }
}