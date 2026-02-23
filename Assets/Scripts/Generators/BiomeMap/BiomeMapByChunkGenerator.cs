using System.Collections.Generic;
using Settings.Biome;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;

namespace Generators.BiomeMap
{
    // TODO use BFS to build a dominance map 
    public class BiomeMapByChunkGenerator : IBiomeMapGenerator
    {
        private const float heightBlendRange = 0.01f;

        public BiomeDensityMap GenerateBiomeMap(int width, int height, BiomeData[] biomes, float[,] heightMap,
            Vector2 sampleCentre)
        {
            BiomeDensityMap biomeDensityMap = new()
            {
                width = width, height = height, primary = new int[width, height], secondary = new int[width, height]
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
                    
                    EvaluateBiomeDensities(wx, wy, heightValue, biomes, offsetX, offsetY, densities);
                    FindTopTwo(densities, out int bestBiome, out int secondBiome);
                    
                    biomeDensityMap.primary[x, y] = bestBiome;
                    biomeDensityMap.secondary[x, y] = secondBiome;
                }
            }
            
            int[,] borderDistance = ComputeDistanceFromBiomeBorder(width, height, biomeDensityMap.primary);
            biomeDensityMap.borderDistance = borderDistance;

            return biomeDensityMap;
        }

        private void EvaluateBiomeDensities(float wx, float wy, float heightValue, BiomeData[] biomes, float[] offsetX,
            float[] offsetY, float[] densities)
        {
            for (int i = 0; i < biomes.Length; i++)
            {
                BiomeData biome = biomes[i];
                float baseNoise = Noise(
                    wx * WorldBiomeGenSettings.biomeFieldFrequency + offsetX[i],
                    wy * WorldBiomeGenSettings.biomeFieldFrequency + offsetY[i]
                ) * biome.globalWeight;

                if (heightValue < biome.minHeight || heightValue > biome.maxHeight)
                    densities[i] = 0f;
                else
                    densities[i] = baseNoise;
            }
        }

        private void FindTopTwo(float[] densities, out int bestBiome, out int secondBiome)
        {
            bestBiome = -1;
            secondBiome = -1;
            float highestDensity = float.MinValue;
            float secondDensity = float.MinValue;
            for (int i = 0; i < densities.Length; i++)
            {
                float density = densities[i];
                if (density > highestDensity)
                {
                    secondDensity = highestDensity;
                    secondBiome = bestBiome;
                    highestDensity = density;
                    bestBiome = i;
                }
                else if (density > secondDensity)
                {
                    secondDensity = density;
                    secondBiome = i;
                }
            }
        }

        private int[,] ComputeDistanceFromBiomeBorder(int width, int height, int[,] primary)
        {
            int[,] distance = new int[width, height];
            bool[,] isBorder = new bool[width, height];
            Queue<Vector2Int> queue = new();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    distance[x, y] = -1;
                    int current = primary[x, y];
                    for (int ny = -1; ny <= 1; ny++)
                    {
                        for (int nx = -1; nx <= 1; nx++)
                        {
                            if (nx == 0 && ny == 0)
                                continue;

                            int px = x + nx;
                            int py = y + ny;

                            if (px < 0 || py < 0 || px >= width || py >= height)
                                continue;

                            if (primary[px, py] != current)
                            {
                                isBorder[x, y] = true;
                                break;
                            }
                        }
                        if (isBorder[x, y])
                            break;
                    }

                    if (isBorder[x, y])
                    {
                        distance[x, y] = 0;
                        queue.Enqueue(new Vector2Int(x, y));
                    }
                }
            }
            
            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                int cx = cell.x;
                int cy = cell.y;

                for (int ny = -1; ny <= 1; ny++)
                {
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        if (nx == 0 && ny == 0)
                            continue;

                        int px = cx + nx;
                        int py = cy + ny;

                        if (px < 0 || py < 0 || px >= width || py >= height)
                            continue;

                        if (distance[px, py] == -1)
                        {
                            distance[px, py] = distance[cx, cy] + 1;
                            queue.Enqueue(new Vector2Int(px, py));
                        }
                    }
                }
            }

            return distance;
        }
        
        private static float Noise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }
    }
}