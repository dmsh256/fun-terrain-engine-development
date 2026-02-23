Shader "Custom/URP/BiomeTerrain"
{
    Properties
    {
        _BiomeAlbedoArray ("Biome Albedo Array", 2DArray) = "" {}
        _TextureScale ("Texture Scale (world)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "Forward"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            TEXTURE2D_ARRAY(_BiomeAlbedoArray);
            SAMPLER(sampler_BiomeAlbedoArray);

            float _TextureScale;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv2 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float biomeId : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float fogCoord : TEXCOORD4;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                float3 worldPos = TransformObjectToWorld(v.positionOS);

                o.positionHCS = TransformWorldToHClip(worldPos);
                o.worldPos = worldPos;
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.biomeId = v.uv2.x;
                o.shadowCoord = TransformWorldToShadowCoord(worldPos);
                o.fogCoord = ComputeFogFactor(o.positionHCS.z);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                int biome = (int)round(i.biomeId);

                int baseIndex = biome * 3;
                int soilIndex  = baseIndex + 0;
                int grassIndex = baseIndex + 1;
                int rockIndex  = baseIndex + 2;
                
                float2 uv = i.worldPos.xz * _TextureScale;

                half4 grass = SAMPLE_TEXTURE2D_ARRAY(
                    _BiomeAlbedoArray, sampler_BiomeAlbedoArray, uv, grassIndex);

                half4 soil = SAMPLE_TEXTURE2D_ARRAY(
                    _BiomeAlbedoArray, sampler_BiomeAlbedoArray, uv, soilIndex);

                half4 rock = SAMPLE_TEXTURE2D_ARRAY(
                    _BiomeAlbedoArray, sampler_BiomeAlbedoArray, uv, rockIndex);

                float3 n = normalize(i.worldNormal);
                float upness = saturate(dot(n, float3(0, 1, 0)));
                float slope = 1.0 - upness;

                float grassEnd = 0.2;
                float soilEnd  = 0.25;
                float feather = 0.1;

                float soilWeight = smoothstep(grassEnd - feather, grassEnd + feather, slope);
                float rockWeight = smoothstep(soilEnd - feather, soilEnd + feather, slope);

                half4 grassSoil = lerp(grass, soil, soilWeight);
                half4 result    = lerp(grassSoil, rock, rockWeight);

                InputData inputData = (InputData)0;

                inputData.positionWS = i.worldPos;
                inputData.normalWS = normalize(i.worldNormal);
                inputData.viewDirectionWS = GetWorldSpaceViewDir(i.worldPos);
                inputData.shadowCoord = i.shadowCoord;
                inputData.fogCoord = 0;
                inputData.vertexLighting = float3(0,0,0);
                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV = 0;
                inputData.shadowMask = 1;

                SurfaceData surfaceData = (SurfaceData)0;

                surfaceData.albedo = result.rgb;
                surfaceData.alpha = 1.0;
                surfaceData.metallic = 0.0;
                surfaceData.smoothness = 0.2;
                surfaceData.normalTS = float3(0,0,1);
                surfaceData.occlusion = 1.0;
                surfaceData.emission = 0.0;
                
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, i.fogCoord);
                
                return color;

            }
            ENDHLSL
        }
    }
}