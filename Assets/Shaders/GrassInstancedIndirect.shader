Shader "Custom/GrassInstancedIndirectLit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap ("Albedo", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _AlphaClip ("Alpha Clip", Float) = 1
        
        _WindDirection ("Wind Direction", Vector) = (1,0,0,0)
        _WindStrength ("Wind Strength", Float) = 0.2
        _WindFrequency ("Wind Frequency", Float) = 1.0
        _WindBendHeight ("Bend Height Influence", Float) = 1.0
    }

    SubShader
    {
        Tags { 
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseMap_ST;
            float4 _BaseColor;
            float _Cutoff;
            float _AlphaClip;

            float4 _WindDirection;
            float _WindStrength;
            float _WindFrequency;
            float _WindBendHeight;
            
            StructuredBuffer<float4x4> _InstanceMatrices;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                uint instanceID   : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                float fogFactor   : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                float4x4 instanceMatrix = _InstanceMatrices[input.instanceID];
                float4 worldPosition = mul(instanceMatrix, float4(input.positionOS, 1.0));
                
                 // wind stage
                float3 windDir = normalize(_WindDirection.xyz);
                
                float heightFactor = saturate(input.positionOS.y * _WindBendHeight);
                float randomOffset = frac(sin(input.instanceID * 12.9898) * 43758.5453);
                float wave = sin(_Time.y * _WindFrequency + randomOffset * 6.28 + worldPosition.x * 0.5 + worldPosition.z * 0.5);
                float bendAmount = wave * _WindStrength * heightFactor;
                // a little gust
                float gust = sin(_Time.y * 0.2) * 0.5 + 0.1;
                bendAmount *= gust;
                
                worldPosition.xyz += windDir * bendAmount;
                //
                
                float3 worldNormal   = normalize(mul((float3x3)instanceMatrix, input.normalOS));

                output.positionWS = worldPosition.xyz;
                output.normalWS   = worldNormal;

                output.positionCS = TransformWorldToHClip(worldPosition.xyz);

                output.uv = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                output.color = input.color;

                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 albedoSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                float4 baseColor = albedoSample * _BaseColor;

                if (_AlphaClip > 0.5)
                    clip(baseColor.a - _Cutoff);

                float3 normalWS = normalize(input.normalWS);

                Light mainLight = GetMainLight();

                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 diffuse = baseColor.rgb * mainLight.color * NdotL;
                float3 ambient = SampleSH(normalWS) * baseColor.rgb;

                float3 finalColor = diffuse + ambient;

                finalColor = MixFog(finalColor, input.fogFactor);

                return float4(finalColor, baseColor.a);
            }
            ENDHLSL
        }
    }
}
