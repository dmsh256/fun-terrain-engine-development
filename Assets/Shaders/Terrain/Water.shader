Shader "Custom/ChunkWater_URP_DepthFog"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0,0.2,0.4,1)
        _ShallowColor ("Shallow Color", Color) = (0.2,0.5,0.6,1)

        _Alpha ("Base Transparency", Range(0,1)) = 0.8
        _DepthFadeDistance ("Depth Fade Distance", Float) = 2.0

        _WaveAmp1 ("Wave Amp 1", Float) = 0.4
        _WaveFreq1 ("Wave Freq 1", Float) = 0.2
        _WaveSpeed1 ("Wave Speed 1", Float) = 1

        _WaveAmp2 ("Wave Amp 2", Float) = 0.2
        _WaveFreq2 ("Wave Freq 2", Float) = 0.5
        _WaveSpeed2 ("Wave Speed 2", Float) = 1.5
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
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma require depthtexture

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _DeepColor;
            float4 _ShallowColor;

            float _Alpha;
            float _DepthFadeDistance;

            float _WaveAmp1;
            float _WaveFreq1;
            float _WaveSpeed1;

            float _WaveAmp2;
            float _WaveFreq2;
            float _WaveSpeed2;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float fogFactor : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            Varyings vert (Attributes input)
            {
                Varyings output;

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                float time = _Time.y;

                float2 dir1 = normalize(float2(1, 0.5));
                float2 dir2 = normalize(float2(-0.7, 1));

                float wave =
                    sin(dot(worldPos.xz, dir1) * _WaveFreq1 + time * _WaveSpeed1) * _WaveAmp1 +
                    sin(dot(worldPos.xz, dir2) * _WaveFreq2 + time * _WaveSpeed2) * _WaveAmp2;

                worldPos.y += wave;

                output.worldPos = worldPos;

                output.positionCS = TransformWorldToHClip(worldPos);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float depthFactor = saturate(input.worldPos.y * 0.1);
                half4 col = lerp(_DeepColor, _ShallowColor, depthFactor);

                float2 uv = input.screenPos.xy / input.screenPos.w;

                float sceneDepth = SampleSceneDepth(uv);
                float sceneEyeDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);

                float waterEyeDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);

                float depthDifference = sceneEyeDepth - waterEyeDepth;

                float depthFade = saturate(depthDifference / _DepthFadeDistance);

                float absorption = 1 - depthFade;

                col.rgb = lerp(_ShallowColor.rgb, _DeepColor.rgb, absorption);
                col.a = _Alpha * depthFade;

                col.rgb = MixFog(col.rgb, input.fogFactor);

                return col;
            }

            ENDHLSL
        }
    }
}