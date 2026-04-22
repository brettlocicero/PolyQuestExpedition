Shader "Custom/URP_Unlit_Transparent_SoftIntersection_NoFog"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
        _Softness ("Intersection Softness", Range(0.001, 5)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 screenPos   : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Softness;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float4 color = tex * _BaseColor;

                // Screen UV
                float2 uv = IN.screenPos.xy / IN.screenPos.w;

                // Scene depth
                float sceneDepth = SampleSceneDepth(uv);
                float sceneLinear = LinearEyeDepth(sceneDepth, _ZBufferParams);

                // Current fragment depth
                float fragLinear = LinearEyeDepth(IN.screenPos.z / IN.screenPos.w, _ZBufferParams);

                // Depth difference
                float diff = sceneLinear - fragLinear;

                // Soft fade
                float fade = saturate(diff / _Softness);

                color.a *= fade;

                return color;
            }

            ENDHLSL
        }
    }

    FallBack Off
}