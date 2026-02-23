using Settings.Biome;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;

namespace Generators.BiomeMap
{
    // TODO doesn't work, fix it
    public class BiomeMapWithBlobsByChunkGenerator : IBiomeMapGenerator
    {
        public BiomeDensityMap GenerateBiomeMap(int width, int height, BiomeData[] biomes, float[,] heightMap, Vector2 sampleCentre)
        {
            BiomeDensityMap biomeDensityMap = new()
            {
                width = width,
                height = height,
                primary = new int[width, height],
                secondary = new int[width, height]
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
            
            for (int y = 0; y < mapWidth; y++)
            {
                for (int x = 0; x < mapLength; x++)
                {
                    float highestDensity = float.MinValue;
                    float secondDensity = float.MinValue;

                    int bestBiome = -1;
                    int secondBiome = -1;

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

                    float[] densities = new float[biomes.Length];
                    float densitySum = 0f;
                    
                    for (int i = 0; i < biomes.Length; i++)
                    {
                        BiomeData biome = biomes[i];

                        if (heightValue < biome.minHeight || heightValue > biome.maxHeight)
                        {
                            densities[i] = 0f;
                            continue;
                        }

                        float biomeValue = Noise(
                            wx * WorldBiomeGenSettings.biomeFieldFrequency + offsetX[i],
                            wy * WorldBiomeGenSettings.biomeFieldFrequency + offsetY[i]
                        ) * biome.globalWeight;

                        float patchNoise = BlobNoise(wx + offsetX[i], wy + offsetY[i], BiomeBlendSettings.patchFrequency);
                        float blobOffset = (patchNoise - 0.5f) * BiomeBlendSettings.patchStrength;
                        biomeValue += blobOffset;

                        if (biomeValue > highestDensity)
                        {
                            secondDensity = highestDensity;
                            secondBiome = bestBiome;

                            highestDensity = biomeValue;
                            bestBiome = i;
                        }
                        else if (biomeValue > secondDensity)
                        {
                            secondDensity = biomeValue;
                            secondBiome = i;
                        }

                        float weightDensity = Mathf.Max(0f, biomeValue);
                        densities[i] = weightDensity;
                        densitySum += weightDensity;
                    }
                    
                    biomeDensityMap.primary[x, y] = bestBiome;
                    biomeDensityMap.secondary[x, y] = secondBiome;
                }
            }

            return biomeDensityMap;
        }

        private static float BlobNoise(float x, float y, float freq)
        {
            float warpX = Mathf.PerlinNoise(x * freq * 0.5f + 1111f, y * freq * 0.5f + 1111f) - 0.5f;
            float warpY = Mathf.PerlinNoise(x * freq * 0.5f + 2222f, y * freq * 0.5f + 2222f) - 0.5f;

            float warped = Mathf.PerlinNoise((x + warpX * 50f) * freq, (y + warpY * 50f) * freq);
            warped = Mathf.Pow(warped, 3f);

            return warped;
        }
        
        private static float Noise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }
    }
}