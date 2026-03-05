using Settings;
using Settings.Biome;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators.BiomeMap
{
    public interface IBiomeMapGenerator
    {
        BiomeDensityMap GenerateBiomeMap(int width, int lenght, float[,] structuralHeightMap, 
            Vector2 sampleCentre, WorldSettings worldSettings, float step = 1f)
        {
            return new BiomeDensityMap();
        }
    }
}