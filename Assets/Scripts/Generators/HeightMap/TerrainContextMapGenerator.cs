using System.Collections.Generic;
using Generators.BiomeMap;
using Settings;
using Settings.Biome;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators.HeightMap
{
    /**
     * Sampling chain: structural height map -> biomes (because they are height dependent)-> biome shaping -> terrain modifiers -> erosion (in the future)
     *
     * Introduce interfaces if you want to make structuralHeightMapGenerator & heightMapProcessor interchangeable
     */
    public class TerrainContextMapGenerator
    {
        private readonly IBiomeMapGenerator biomeGenerator;
        private readonly StructuralHeightMapGenerator structuralHeightMapGenerator = new();
        private readonly HeightMapProcessor heightMapProcessor = new();
        private readonly WorldSettings worldSettings;
        
        public TerrainContextMapGenerator(WorldSettings worldSettings)
        {
            this.worldSettings = worldSettings;
            biomeGenerator = BiomeGeneratorFactory.GetBiomeMapGenerator(worldSettings);
        }
        
        public TerrainContextMap GenerateTerrainContextMap(int width, int length, HeightMapSettings heightMapSettings, Vector2 sampleCentre, BiomeData[] biomes, List<IHeightMapModifier> heightModifiers = null)
        {
            float[,] structuralHeight = structuralHeightMapGenerator.GenerateStructuralHeightMap(width, length, heightMapSettings, sampleCentre, worldSettings.worldStep);

            BiomeDensityMap biomeDensityMap =
                biomeGenerator.GenerateBiomeMap(width, length, biomes, structuralHeight, sampleCentre, worldSettings);
            
            return heightMapProcessor.ProcessHeight(structuralHeight, heightMapSettings, sampleCentre, biomeDensityMap, biomes);
        }
    }
}