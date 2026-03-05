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

        public BiomeDensityMap GenerateBiomeMap(int width, int lenght, float[,] heightMap, Vector2 sampleCentre,
            WorldSettings worldSettings, float step = 1f)
        {
            BiomeDensityMap biomeDensityMap = new()
            {
                width = width,
                height = lenght,
                primary = new int[width, lenght],
                secondary = new int[width, lenght],
                dominance = new float[width, lenght]
            };
            
            int splatResolution = width - 2; // TODO cleanup
            int splatCount = Mathf.CeilToInt(worldSettings.biomes.Length / 4f);
            biomeDensityMap.splatMap = new Color[splatCount][];
            for (int i = 0; i < splatCount; i++)
                biomeDensityMap.splatMap[i] = new Color[splatResolution * splatResolution];

            System.Random rng = new(WorldContextSettings.Seed);

            float[] offsetX = new float[worldSettings.biomes.Length];
            float[] offsetY = new float[worldSettings.biomes.Length];
            for (int i = 0; i < worldSettings.biomes.Length; i++)
            {
                offsetX[i] = rng.Next(-100000, 100000);
                offsetY[i] = rng.Next(-100000, 100000);
            }

            float[] densities = new float[worldSettings.biomes.Length];

            for (int y = 0; y < lenght; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float worldX = (x * step + sampleCentre.x) / worldSettings.worldStructureScale;
                    float worldY = (y * step + sampleCentre.y) / worldSettings.worldStructureScale;
                    
                    float warpX =
                        Noise(worldX * WorldBiomeGenSettings.biomeWarpFrequency + 1000f,
                              worldY * WorldBiomeGenSettings.biomeWarpFrequency + 1000f)
                        * WorldBiomeGenSettings.biomeWarpStrength;

                    float warpY =
                        Noise(worldX * WorldBiomeGenSettings.biomeWarpFrequency + 2000f,
                              worldY * WorldBiomeGenSettings.biomeWarpFrequency + 2000f)
                        * WorldBiomeGenSettings.biomeWarpStrength;

                    float warpWorldX = worldX + warpX;
                    float warpWorldY = worldY + warpY;

                    float heightValue = heightMap[x, y];

                    EvaluateBiomeDensities(worldSettings, warpWorldX, warpWorldY, heightValue, worldSettings.biomes, offsetX, offsetY, densities);
                    FindTopTwo(densities, out int bestBiome, out int secondBiome, out float highestDensity, out float secondDensity);

                    biomeDensityMap.primary[x, y] = bestBiome;
                    biomeDensityMap.secondary[x, y] = secondBiome;

                    float sum = highestDensity + secondDensity;
                    biomeDensityMap.dominance[x, y] = sum > 0f ? highestDensity / sum : 1f;

                    int splatIndex = bestBiome / 4;
                    int channel    = bestBiome % 4;

                    Color pixel = Color.clear;
                    pixel[channel] = 1f;
                    if (x > 0 && x < width - 1 && y > 0 && y < lenght - 1) // ignore stitch vertices
                    {
                        biomeDensityMap.splatMap[splatIndex][(y - 1) * splatResolution + (x - 1)] = pixel;
                    }
                }
            }

            return biomeDensityMap;
        }
        
        private void EvaluateBiomeDensities(WorldSettings worldSettings, float warpWorldX, float warpWorldY, float heightValue, BiomeData[] biomes, float[] offsetX, float[] offsetY, float[] densities)
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

                float biomeNoise = Noise(warpWorldX * WorldBiomeGenSettings.biomeFieldFrequency + offsetX[i],
                        warpWorldY * WorldBiomeGenSettings.biomeFieldFrequency + offsetY[i]);

                float density = biomeNoise * biome.globalWeight * heightFactor;
                if (biome.isWater)
                    density *= waterFactor;
                else
                    density *= 1f - waterFactor;

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

            if (bestBiome == -1)
            {
                bestBiome = 0;
                secondBiome = 0;
                highest = 1f;
                second = 0f;
            }
        }

        private static float Noise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }
    }
}