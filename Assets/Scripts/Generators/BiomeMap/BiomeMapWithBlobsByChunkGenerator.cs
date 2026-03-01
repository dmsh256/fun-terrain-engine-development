using Settings;
using Settings.Biome;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;

namespace Generators.BiomeMap
{
    // TODO under construction
    public class BiomeMapWithBlobsByChunkGenerator : IBiomeMapGenerator
    {
        private const float heightBlendRange = 0.01f;

        public BiomeDensityMap GenerateBiomeMap(int width, int height, BiomeData[] biomes, float[,] heightMap, Vector2 sampleCentre, WorldSettings worldSettings)
        {
            BiomeDensityMap biomeDensityMap = new()
            {
                width = width,
                height = height,
                primary = new int[width, height],
                secondary = new int[width, height],
                dominance = new float[width, height]
            };

            int mapLength = heightMap.GetLength(0);
            int mapWidth = heightMap.GetLength(1);

            System.Random rng = new(WorldContextSettings.Seed);

            float[] offsetX = new float[biomes.Length];
            float[] offsetY = new float[biomes.Length];

            for (int i = 0; i < biomes.Length; i++)
            {
                offsetX[i] = rng.Next(-100000, 100000);
                offsetY[i] = rng.Next(-100000, 100000);
            }

            float[] densities = new float[biomes.Length];
            for (int y = 0; y < mapWidth; y++)
            {
                for (int x = 0; x < mapLength; x++)
                {
                    float fx = x + sampleCentre.x;
                    float fy = y + sampleCentre.y;

                    float warpX = Noise(
                        fx * WorldBiomeGenSettings.biomeWarpFrequency + 1000f,
                        fy * WorldBiomeGenSettings.biomeWarpFrequency + 1000f
                    ) * WorldBiomeGenSettings.biomeWarpStrength;

                    float warpY = Noise(
                        fx * WorldBiomeGenSettings.biomeWarpFrequency + 2000f,
                        fy * WorldBiomeGenSettings.biomeWarpFrequency + 2000f
                    ) * WorldBiomeGenSettings.biomeWarpStrength;

                    float wx = fx + warpX;
                    float wy = fy + warpY;

                    float heightValue = heightMap[x, y];

                    EvaluateBiomeDensities(wx, wy, heightValue, biomes, offsetX, offsetY, densities, worldSettings);
                    FindTopTwo(densities, out int bestBiome, out int secondBiome, out float highest, out float second);

                    biomeDensityMap.primary[x, y] = bestBiome;
                    biomeDensityMap.secondary[x, y] = secondBiome;
                    biomeDensityMap.dominance[x, y] = highest - second;
                }
            }

            return biomeDensityMap;
        }

        private void EvaluateBiomeDensities(float wx, float wy, float heightValue, BiomeData[] biomes, float[] offsetX, float[] offsetY, float[] densities, WorldSettings worldSettings)
        {
            float shorelineBlend = WorldBiomeGenSettings.shorelineBlend;
            float contrast = WorldBiomeGenSettings.biomeContrast;
            float waterFactor = Mathf.Clamp01((worldSettings.waterLevel - heightValue) / shorelineBlend);

            for (int i = 0; i < biomes.Length; i++)
            {
                BiomeData biome = biomes[i];
                float heightFactor = 1f;
                if (!biome.isWater)
                {
                    if (heightValue < biome.minHeight)
                    {
                        float delta = biome.minHeight - heightValue;
                        heightFactor = 1f - Mathf.Clamp01(delta / heightBlendRange);
                    }
                    else if (heightValue > biome.maxHeight)
                    {
                        float delta = heightValue - biome.maxHeight;
                        heightFactor = 1f - Mathf.Clamp01(delta / heightBlendRange);
                    }
                }

                if (heightFactor <= 0f)
                {
                    densities[i] = 0f;
                    continue;
                }

                float baseNoise = Noise(wx * WorldBiomeGenSettings.biomeFieldFrequency + offsetX[i], wy * WorldBiomeGenSettings.biomeFieldFrequency + offsetY[i]) 
                                  * biome.globalWeight;

                float patchNoise = BlobNoise(wx + offsetX[i], wy + offsetY[i], BiomeBlendSettings.patchFrequency);
                float blobOffset = (patchNoise - 0.5f) * BiomeBlendSettings.patchStrength;
                float density = baseNoise * (1f + blobOffset);

                if (biome.isWater)
                    density *= waterFactor;
                else
                    density *= (1f - waterFactor);

                density = Mathf.Max(0f, density);
                densities[i] = Mathf.Pow(density, contrast);
            }
        }

        private void FindTopTwo(float[] densities, out int bestBiome, out int secondBiome, out float highest, out float second)
        {
            bestBiome = -1;
            secondBiome = -1;
            highest = float.MinValue;
            second = float.MinValue;

            for (int i = 0; i < densities.Length; i++)
            {
                float value = densities[i];

                if (value > highest)
                {
                    second = highest;
                    secondBiome = bestBiome;

                    highest = value;
                    bestBiome = i;
                }
                else if (value > second)
                {
                    second = value;
                    secondBiome = i;
                }
            }

            if (secondBiome == -1)
                second = 0f;
        }

        private static float BlobNoise(float x, float y, float freq)
        {
            float warpX = Mathf.PerlinNoise(x * freq * 0.5f + 1111f,
                y * freq * 0.5f + 1111f) - 0.5f;

            float warpY = Mathf.PerlinNoise(x * freq * 0.5f + 2222f,
                y * freq * 0.5f + 2222f) - 0.5f;

            float warped = Mathf.PerlinNoise(
                (x + warpX * 50f) * freq,
                (y + warpY * 50f) * freq
            );

            return Mathf.Pow(warped, 3f);
        }

        private static float Noise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }
    }
}