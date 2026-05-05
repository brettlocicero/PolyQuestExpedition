Shader "Custom/URP/LitGrassRockColors_Fog"
{
    Properties
    {
        _GrassColor ("Grass Color", Color) = (0.2, 0.8, 0.2, 1)
        _RockColor  ("Rock Color", Color)  = (0.5, 0.5, 0.5, 1)

        _BlendStart ("Blend Start", Range(0,1)) = 0.3
        _BlendEnd   ("Blend End", Range(0,1)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float fogFactor   : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _GrassColor;
                float4 _RockColor;
                float _BlendStart;
                float _BlendEnd;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs nrm = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;
                OUT.normalWS = nrm.normalWS;

                // 🌫️ IMPORTANT: fog factor must be computed in vertex
                OUT.fogFactor = ComputeFogFactor(pos.positionCS.z);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);

                float slope = saturate(normal.y);
                float blend = smoothstep(_BlendStart, _BlendEnd, slope);

                float3 albedo = lerp(_RockColor.rgb, _GrassColor.rgb, blend);

                // ---- Lighting ----
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normal, mainLight.direction));

                float3 color = albedo * mainLight.color * NdotL;
                color += SampleSH(normal) * albedo; // ambient

                // 🌫️ APPLY FOG (this is the missing piece)
                color = MixFog(color, IN.fogFactor);

                return half4(color, 1.0);
            }

            ENDHLSL
        }
    }
}