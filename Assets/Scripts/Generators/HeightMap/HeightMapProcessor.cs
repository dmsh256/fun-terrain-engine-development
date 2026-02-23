using System.Collections.Generic;
using Generators.BiomeMap.BiomeTerrainShapers;
using Settings;
using Settings.Biome;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators.HeightMap
{
    public class HeightMapProcessor
    {
        private const float DefaultHeightMultiplier = 1f;
        
        private List<IHeightMapModifier> heightModifiers;
        
        public TerrainContextMap ProcessHeight(float[,] values, GlobalHeightMapSettings settings, Vector2 sampleCentre, BiomeDensityMap biomeMap, BiomeData[] biomes)
        {
            int width = values.GetLength(0);
            int length = values.GetLength(1);

            float heightMultiplier = settings.useHeightMultiplier
                ? settings.heightMultiplier
                : DefaultHeightMultiplier;

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            if (heightModifiers != null) // no branching in the hot path
            {
                for (int y = 0; y < length; y++)
                {
                    float worldZ = sampleCentre.y + y;
                    for (int x = 0; x < width; x++)
                    {
                        float worldX = sampleCentre.x + x;
                        float height = values[x, y];
                        
                        height = ApplyBiomeShaping(biomes, biomeMap, x, y, worldX, worldZ, height);
                        height = ApplyModifiers(worldX, worldZ, height);
                        
                        height *= heightMultiplier;
                        values[x, y] = height;
    
                        if (height > maxValue) maxValue = height;
                        if (height < minValue) minValue = height;
                    }
                }
            }
            else
            {
                for (int y = 0; y < length; y++)
                {
                    float worldZ = sampleCentre.y + y;
                    for (int x = 0; x < width; x++)
                    {
                        float worldX = sampleCentre.x + x;
                        float height = values[x, y];
                        
                        height = ApplyBiomeShaping(biomes, biomeMap, x, y, worldX, worldZ, height);
                        
                        height *= heightMultiplier;
                        values[x, y] = height;
    
                        if (height > maxValue) maxValue = height;
                        if (height < minValue) minValue = height;
                    }
                }
            }
            
            return new TerrainContextMap(
                new HeightMap(values, minValue, maxValue, heightMultiplier),
                biomeMap
            );
        }

        private static float ApplyBiomeShaping(BiomeData[] biomes, BiomeDensityMap biomeMap, int x, int y, float worldX, float worldZ, float height)
        {
            int primary = biomeMap.primary[x, y];
            if (primary == -1)
                return height;

            TerrainContext terrainContext = new()
            {
                worldX = worldX,
                worldZ = worldZ,
                height = height,
                biomeMap = biomeMap,
                biomes = biomes,
                primaryBiome = primary,
                secondaryBiome = biomeMap.secondary[x, y],
                dominance = biomeMap.dominance[x, y]
            };

            BiomeTerrainShaper shaper = biomes[primary].terrainShaper;
            
            return shaper.Shape(terrainContext);
        }

        private float ApplyModifiers(float worldX, float worldZ, float height)
        {
            for (int i = 0; i < heightModifiers.Count; i++)
            {
                height = heightModifiers[i].Evaluate(worldX, worldZ, height);
            }

            return height;
        }
    }
}