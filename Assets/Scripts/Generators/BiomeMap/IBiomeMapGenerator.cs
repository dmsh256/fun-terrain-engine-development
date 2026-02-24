using Settings;
using Settings.Biome;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators.BiomeMap
{
    public interface IBiomeMapGenerator
    {
        BiomeDensityMap GenerateBiomeMap(int width, int height, BiomeData[] biomes, float[,] structuralHeightMap, Vector2 sampleCentre, WorldSettings worldSettings)
        {
            return new BiomeDensityMap();
        }
    }
}