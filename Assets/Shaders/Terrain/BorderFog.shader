Shader "Custom/WorldBorderFogWithParticles"
{
    Properties
    {
        _Color ("Fog Color", Color) = (0.6, 0.2, 1, 1)

        _Intensity ("Intensity", Float) = 2.0
        _NoiseScale ("Fog Noise Scale", Float) = 2.0

        _Speed ("Fog Speed", Float) = 0.1

        _VerticalFade ("Vertical Fade (0-0.5)", Range(0,0.5)) = 0.15
        
        _TimeOffset ("Time Offset", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float4 _Color;

            float _Intensity;
            float _NoiseScale;

            float _Speed;

            float _VerticalFade;
            
            float _TimeOffset;

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = Hash(i);
                float b = Hash(i + float2(1,0));
                float c = Hash(i + float2(0,1));
                float d = Hash(i + float2(1,1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
            }

            float FBM(float2 p)
            {
                float v = 0.0;
                v += Noise(p);
                v += Noise(p * 2.0) * 0.5;
                v += Noise(p * 4.0) * 0.25;
                return v / 1.75;
            }

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 wp = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(wp);
                o.uv = v.uv;
                o.worldPos = wp;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                // =======================
                // FOG LAYER
                // =======================

                float2 fogUV = uv * _NoiseScale;
                fogUV.y += (_Time.y + _TimeOffset) * _Speed;

                float fog = FBM(fogUV);
                fog = smoothstep(0.3, 0.7, fog);
                fog = pow(fog, 1.4);
                
                // =======================
                // VERTICAL FADE
                // =======================

                float fadeBottom = smoothstep(0.0, _VerticalFade, uv.y);
                float fadeTop = 1.0 - smoothstep(1.0 - _VerticalFade, 1.0, uv.y);
                float heightFade = fadeBottom * fadeTop;

                // =======================
                // COMBINE
                // =======================

                float3 color = _Color.rgb * fog;

                float energy = fog * heightFade * _Intensity;

                return float4(color * heightFade * _Intensity, energy);
            }

            ENDHLSL
        }
    }
}