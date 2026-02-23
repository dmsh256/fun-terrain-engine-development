Shader "Custom/WorldBorderParticleCurtain"
{
    Properties
    {
        _Color ("Particle Color", Color) = (1, 0.85, 0.4, 1)
        _Columns ("Columns", Float) = 120
        _ParticlesPerColumn ("Particles Per Column", Range(1,24)) = 8
        _ColumnWidth ("Column Width", Float) = 0.8
        _Speed ("Rise Speed", Float) = 0.4
        _Size ("Particle Size", Float) = 0.08
        _Glow ("Glow Intensity", Float) = 3.0
        _VerticalFade ("World Vertical Safe Zone", Float) = 2.0
        _TopFadeRandom("Top fade randomness", Float) = 1.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One
        ZWrite Off
        Cull Back

        Pass
        { 
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define MAX_PARTICLES 24

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
            float _Columns;
            float _ParticlesPerColumn;
            float _ColumnWidth;
            float _Speed;
            float _Size;
            float _Glow;
            float _VerticalFade;
            float _TopFadeRandom;
            
            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float ColumnHeightMask(float columnId, float worldY, float minY, float maxY)
            {
                float columnNoise = Hash(float2(columnId, 17.31));
                float columnTop = lerp(maxY - 1.0, 1 - maxY / minY, columnNoise);

                return 1.0 - smoothstep(columnTop - 2.0, columnTop, worldY);
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
                
                float x = uv.x * _Columns;
                float columnId = floor(x);
                float columnX = frac(x) - 0.5;

                float energy = 0.0;
                float time = _Time.y;
                
                [unroll]
                for (int p = 0; p < MAX_PARTICLES; p++)
                {
                    if (p >= _ParticlesPerColumn)
                        break;

                    float seed = Hash(float2(columnId, p));

                    float particleX =
                        columnX +
                        (seed - 0.5) * (_ColumnWidth / _Columns);

                    float baseY = Hash(float2(p, columnId));
                    float speed = _Speed * lerp(0.6, 1.4, seed);

                    float lifetime = frac(time * speed + seed);
                    float particleY = frac(baseY + lifetime);
                    float lifeAlpha = smoothstep(0.1, 0.3, lifetime) * (1.0 - smoothstep(0.6, 0.85, lifetime));

                    float size = _Size * lerp(0.7, 1.3, seed);
                    float dx = abs(particleX);
                    float dy = abs(uv.y - particleY);
                    dy *= 0.8;

                    float dist = sqrt(dx * dx + dy * dy);
                    float particle = smoothstep(size, 0.0, dist);

                    energy = max(energy, particle * lifeAlpha);
                }

                energy *= _Glow;
                
                float centerY = unity_ObjectToWorld._m13;
                float halfY   = unity_ObjectToWorld._m13 * 0.5;

                float minY = centerY - halfY;
                float maxY = centerY + halfY;

                float safeMinY = minY + _VerticalFade;
                float seed = Hash(float2(columnId, uv.x));
                float particleTopOffset = lerp(0.0, _TopFadeRandom, seed);
                float safeMaxY = maxY - _VerticalFade - particleTopOffset;
                
                float worldFade =
                    smoothstep(minY, safeMinY, i.worldPos.y) *
                    (1.0 - smoothstep(safeMaxY, maxY, i.worldPos.y));

                energy *= worldFade;

                float columnFade = ColumnHeightMask(columnId, i.worldPos.y, minY, maxY);
                energy *= columnFade;
                
                return float4(_Color.rgb * energy, energy);
            }
            ENDHLSL
        }
    }
}
