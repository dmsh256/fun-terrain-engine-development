Shader "Custom/URP/BiomeTerrain"
{
    Properties
    {
        _BiomeAlbedoArray ("Biome Albedo Array", 2DArray) = "" {}
        _SplatMap0 ("Splat Map 0", 2D) = "white" {}
        _SplatMap1 ("Splat Map 1", 2D) = "black" {}
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
            Tags { "LightMode"="UniversalForward" }

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

            TEXTURE2D(_SplatMap0);
            SAMPLER(sampler_SplatMap0);

            TEXTURE2D(_SplatMap1);
            SAMPLER(sampler_SplatMap1);

            float _TextureScale;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float fogCoord     : TEXCOORD4;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                float3 worldPos = TransformObjectToWorld(v.positionOS);

                o.positionHCS = TransformWorldToHClip(worldPos);
                o.worldPos = worldPos;
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.shadowCoord = TransformWorldToShadowCoord(worldPos);
                o.fogCoord = ComputeFogFactor(o.positionHCS.z);
                o.uv = v.uv;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float4 weights0 = SAMPLE_TEXTURE2D_LOD(_SplatMap0, sampler_SplatMap0, i.uv, 0);
                float4 weights1 = SAMPLE_TEXTURE2D_LOD(_SplatMap1, sampler_SplatMap1, i.uv, 0);

                float2 worldUV = i.worldPos.xz * _TextureScale;

                float3 n = normalize(i.worldNormal);
                float upness = saturate(dot(n, float3(0,1,0)));
                float slope = 1.0 - upness;

                float soilWeight = smoothstep(0.1, 0.3, slope);
                float rockWeight = smoothstep(0.15, 0.35, slope);

                int biomeIndex = 0;

                if      (weights0.r > 0) biomeIndex = 0;
                else if (weights0.g > 0) biomeIndex = 1;
                else if (weights0.b > 0) biomeIndex = 2;
                else if (weights0.a > 0) biomeIndex = 3;
                else if (weights1.r > 0) biomeIndex = 4;

                int baseIndex = biomeIndex * 3;

                half4 soil = SAMPLE_TEXTURE2D_ARRAY(
                    _BiomeAlbedoArray, sampler_BiomeAlbedoArray,
                    worldUV, baseIndex + 0);

                half4 grass = SAMPLE_TEXTURE2D_ARRAY(
                    _BiomeAlbedoArray, sampler_BiomeAlbedoArray,
                    worldUV, baseIndex + 1);

                half4 rock = SAMPLE_TEXTURE2D_ARRAY(
                    _BiomeAlbedoArray, sampler_BiomeAlbedoArray,
                    worldUV, baseIndex + 2);

                half4 biomeColor =
                    lerp(lerp(grass, soil, soilWeight), rock, rockWeight);

                InputData inputData = (InputData)0;
                inputData.positionWS = i.worldPos;
                inputData.normalWS = n;
                inputData.viewDirectionWS = GetWorldSpaceViewDir(i.worldPos);
                inputData.shadowCoord = i.shadowCoord;
                inputData.fogCoord = 0;
                inputData.bakedGI = SampleSH(n);
                inputData.shadowMask = 1;

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = biomeColor.rgb;
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