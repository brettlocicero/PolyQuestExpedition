Shader "Custom/URP/SoftPortalSphere"
{
    Properties
    {
        [HDR]_InnerColor ("Inner Color", Color) = (0.2,0.6,1,1)
        [HDR]_OuterColor ("Outer Color", Color) = (1,0.2,1,1)

        _PortalScale ("Portal Scale", Float) = 4
        _PortalSpeed ("Portal Speed", Float) = 1

        _FresnelPower ("Fresnel Power", Range(0.1,10)) = 4
        _FresnelIntensity ("Fresnel Intensity", Range(0,20)) = 4

        _DeformAmount ("Deform Amount", Range(0,1)) = 0.15
        _DeformSpeed ("Deform Speed", Float) = 1

        _EdgeSoftness ("Edge Softness", Range(0.1,8)) = 3

        _NoiseStrength ("Noise Strength", Range(0,2)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off

        Pass
        {
            Name "Forward"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)

            float4 _InnerColor;
            float4 _OuterColor;

            float _PortalScale;
            float _PortalSpeed;

            float _FresnelPower;
            float _FresnelIntensity;

            float _DeformAmount;
            float _DeformSpeed;

            float _EdgeSoftness;
            float _NoiseStrength;

            CBUFFER_END

            //------------------------------------------------
            // Noise Functions
            //------------------------------------------------

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 34.45);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1,0));
                float c = hash(i + float2(0,1));
                float d = hash(i + float2(1,1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a,b,u.x)
                     + (c-a) * u.y * (1-u.x)
                     + (d-b) * u.x * u.y;
            }

            //------------------------------------------------
            // Vertex
            //------------------------------------------------

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float time = _Time.y * _DeformSpeed;

                float3 pos = IN.positionOS.xyz;

                float deform =
                    sin(pos.x * 6 + time) *
                    sin(pos.y * 5 - time * 1.3) *
                    sin(pos.z * 7 + time * 0.8);

                pos += IN.normalOS * deform * _DeformAmount;

                VertexPositionInputs posInput =
                    GetVertexPositionInputs(pos);

                VertexNormalInputs normalInput =
                    GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInput.positionCS;
                OUT.positionWS = posInput.positionWS;
                OUT.normalWS = normalize(normalInput.normalWS);
                OUT.uv = IN.uv;

                return OUT;
            }

            //------------------------------------------------
            // Fragment
            //------------------------------------------------

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);

                float3 viewDir =
                    normalize(_WorldSpaceCameraPos - IN.positionWS);

                //---------------------------------------------
                // Fresnel
                //---------------------------------------------

                float fresnel =
                    pow(
                        1.0 - saturate(dot(viewDir, normal)),
                        _FresnelPower
                    );

                //---------------------------------------------
                // Portal UV
                //---------------------------------------------

                float2 uv = IN.uv * 2.0 - 1.0;

                float r = length(uv);

                float angle =
                    atan2(uv.y, uv.x);

                float time =
                    _Time.y * _PortalSpeed;

                angle += time;

                angle +=
                    sin(r * 10 - time * 4) * 0.25;

                float2 swirlUV =
                    float2(
                        cos(angle),
                        sin(angle)
                    ) * r * _PortalScale;

                //---------------------------------------------
                // Layered Noise
                //---------------------------------------------

                float n = 0;

                n += noise(swirlUV + time);
                n += noise(swirlUV * 2 - time * 1.5) * 0.5;
                n += noise(swirlUV * 4 + time * 3) * 0.25;

                n /= 1.75;

                n = saturate(n);

                //---------------------------------------------
                // Portal Color
                //---------------------------------------------

                float3 portalColor =
                    lerp(
                        _InnerColor.rgb,
                        _OuterColor.rgb,
                        n
                    );

                //---------------------------------------------
                // Energy Rim
                //---------------------------------------------

                float rimNoise =
                    noise(swirlUV * 4 + time * 2);

                float rim =
                    fresnel *
                    rimNoise *
                    _FresnelIntensity;

                portalColor +=
                    _OuterColor.rgb *
                    rim;

                //---------------------------------------------
                // Main Fresnel Glow
                //---------------------------------------------

                portalColor +=
                    _OuterColor.rgb *
                    fresnel *
                    (_FresnelIntensity * 0.5);

                //---------------------------------------------
                // Soft Edge Fade
                //---------------------------------------------

                float edgeFade =
                    pow(
                        saturate(1.0 - fresnel),
                        _EdgeSoftness
                    );

                float centerFade =
                    smoothstep(
                        1.0,
                        0.25,
                        r
                    );

                float alpha =
                    edgeFade *
                    centerFade;

                alpha *= (0.7 + n * 0.5);

                alpha = saturate(alpha);

                //---------------------------------------------
                // Bright Core
                //---------------------------------------------

                float core =
                    smoothstep(
                        0.6,
                        0.0,
                        r
                    );

                portalColor +=
                    _InnerColor.rgb *
                    core *
                    0.5;

                return half4(portalColor, alpha);
            }

            ENDHLSL
        }
    }
}