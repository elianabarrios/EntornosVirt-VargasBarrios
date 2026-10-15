Shader "Custom/Pared"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-10" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ColorMask 0
            ZWrite Off
            Cull Off
            Stencil { Ref 1 Comp Always Pass Replace }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct A { float4 p : POSITION; };
            struct V { float4 p : SV_POSITION; };

            V vert(A i)
            {
                V o;
                o.p = TransformObjectToHClip(i.p.xyz);
                return o;
            }

            half4 frag(V i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}