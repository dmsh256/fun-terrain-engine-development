using Settings;
using Settings.Biome;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;

namespace Generators.BiomeMap
{
    public class BiomeMapByChunkGenerator : IBiomeMapGenerator
    {
        private const float heightBlendRange = 0.01f;

        public BiomeDensityMap GenerateBiomeMap(int width, int height, BiomeData[] biomes, float[,] heightMap,
            Vector2 sampleCentre, WorldSettings worldSettings)
        {
            BiomeDensityMap biomeDensityMap = new()
            {
                width = width, height = height, primary = new int[width, height], secondary = new int[width, height],
                dominance = new float[width, height]
            };
            
            int mapLength = heightMap.GetLength(0);
            int mapWidth = heightMap.GetLength(1);
            
            System.Random rng = new(WorldContext.Seed);
            
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
                    float warpX =
                        Noise(fx * WorldBiomeGenSettings.biomeWarpFrequency + 1000f,
                            fy * WorldBiomeGenSettings.biomeWarpFrequency + 1000f) *
                        WorldBiomeGenSettings.biomeWarpStrength;
                    
                    float warpY =
                        Noise(fx * WorldBiomeGenSettings.biomeWarpFrequency + 2000f,
                            fy * WorldBiomeGenSettings.biomeWarpFrequency + 2000f) *
                        WorldBiomeGenSettings.biomeWarpStrength;
                    
                    float wx = fx + warpX;
                    float wy = fy + warpY;
                    float heightValue = heightMap[x, y];
                    
                    EvaluateBiomeDensities(worldSettings, wx, wy, heightValue, biomes, offsetX, offsetY, densities);
                    FindTopTwo(densities, out int bestBiome, out int secondBiome, out float highestDensity,
                        out float secondDensity);
                    
                    biomeDensityMap.primary[x, y] = bestBiome;
                    biomeDensityMap.secondary[x, y] = secondBiome;
                    
                    float sum = highestDensity + secondDensity;
                    biomeDensityMap.dominance[x, y] = sum > 0f ? highestDensity / sum : 1f;
                }
            }

            return biomeDensityMap;
        }

        private void EvaluateBiomeDensities(WorldSettings worldSettings, float wx, float wy, float heightValue, BiomeData[] biomes, float[] offsetX, float[] offsetY, float[] densities)
        {
            float shorelineBlend = WorldBiomeGenSettings.shorelineBlend;
            float waterFactor = Mathf.Clamp01((worldSettings.waterLevel - heightValue) / shorelineBlend);
            
            for (int i = 0; i < biomes.Length; i++)
            {
                BiomeData biome = biomes[i];
                float heightFactor = 1f;
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

                if (heightFactor <= 0f)
                {
                    densities[i] = 0f;
                    continue;
                }

                float biomeNoise =
                    Noise(wx * WorldBiomeGenSettings.biomeFieldFrequency + offsetX[i],
                        wy * WorldBiomeGenSettings.biomeFieldFrequency + offsetY[i]);

                float density = biomeNoise * biome.globalWeight * heightFactor;

                if (biome.isWater)
                    density *= waterFactor;
                else
                    density *= (1f - waterFactor);

                densities[i] = Mathf.Max(0f, density);
            }
            
            float contrast = WorldBiomeGenSettings.biomeContrast;
            for (int i = 0; i < biomes.Length; i++)
            {
                densities[i] = Mathf.Pow(densities[i], contrast);
            }
        }

        private void FindTopTwo(float[] densities, out int bestBiome, out int secondBiome, out float highest,
            out float second)
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

        private static float Noise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }
    }
}