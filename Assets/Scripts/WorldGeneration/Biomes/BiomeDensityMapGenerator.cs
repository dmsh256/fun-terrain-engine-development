using Generators.HeightMap;
using Settings.Biome;
using UnityEngine;

//TODO place into generators folder?
namespace WorldGeneration.Biomes
{
    public static class BiomeDensityMapGenerator
    {
        public static BiomeDensityMap BiomeDensityMapFromContext(
            int width,
            int height,
            BiomeData[] biomes,
            HeightMap heightMap
        )
        {
            BiomeDensityMap biomeDensityMap = new()
            {
                width = width,
                height = height,
                primary = new int[width, height]
            };

            int mapLength = heightMap.values.GetLength(0);
            int mapWidth = heightMap.values.GetLength(1);

            System.Random rng = new(0);

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
                    int bestBiome = -1;

                    float fx = x;
                    float fy = y;

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

                    float heightValue = heightMap.values[x, y];
                    for (int i = 0; i < biomes.Length; i++)
                    {
                        BiomeData biome = biomes[i];

                        if (heightValue < biome.minHeight || heightValue > biome.maxHeight)
                            continue;
                        
                        float biomeValue = Noise(
                            wx * WorldBiomeGenSettings.biomeFieldFrequency + offsetX[i],
                            wy * WorldBiomeGenSettings.biomeFieldFrequency + offsetY[i]
                        ) * biome.globalWeight;

                        if (biomeValue > highestDensity)
                        {
                            highestDensity = biomeValue;
                            bestBiome = i;
                        }
                    }

                    biomeDensityMap.primary[x, y] = bestBiome;
                }
            }

            return biomeDensityMap;
        }

        private static float Noise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }
    }
}