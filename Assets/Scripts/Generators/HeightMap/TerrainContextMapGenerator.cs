using System.Collections.Generic;
using Generators.BiomeMap;
using Settings;
using Settings.Biome;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;
using WorldGeneration.WorldStructuralModifiers;

namespace Generators.HeightMap
{
    /**
     * Sampling chain: structural height map -> structural map modifiers -> biomes (because they are height dependent)
     *     -> biome shaping -> terrain modifiers (low res) -> erosion (in the future)
     *
     * Introduce interfaces if you want to make structuralHeightMapGenerator & heightMapProcessor interchangeable.
     *
     * Low res sampling usually don't require worldStructure, it is supposed to make the worldStructure
     */
    public class TerrainContextMapGenerator
    {
        private readonly IBiomeMapGenerator biomeGenerator;
        private readonly StructuralHeightMapGenerator structuralHeightMapGenerator = new();
        private readonly HeightMapProcessor heightMapProcessor = new();
        private readonly WorldSettings worldSettings;
        private List<IStructuralHeightModifier> structuralHeightModifiersList;
        private List<IHeightMapModifier> heightModifiersList;
        
        public TerrainContextMapGenerator(WorldSettings worldSettings)
        {
            this.worldSettings = worldSettings;
            biomeGenerator = BiomeGeneratorFactory.GetBiomeMapGenerator(worldSettings);
        }

        public void SetStructuralModifiers(List<IStructuralHeightModifier> structuralHeightModifiers)
        {
            structuralHeightModifiersList = structuralHeightModifiers;
        }

        public void SetHeightModifiers(List<IHeightMapModifier> heightModifiers)
        {
            heightModifiersList = heightModifiers;
        }
        
        public TerrainContextMap GenerateTerrainContextMap(int width, int length, HeightMapSettings heightMapSettings, 
            Vector2 sampleCentre, BiomeData[] biomes, float step = 1f)
        {
            float[,] structuralHeight = 
                structuralHeightMapGenerator.GenerateStructuralHeightMap(width, length, heightMapSettings, sampleCentre, 
                    structuralHeightModifiersList, step);

            BiomeDensityMap biomeDensityMap =
                biomeGenerator.GenerateBiomeMap(width, length, biomes, structuralHeight, sampleCentre, worldSettings, step);
            
            heightMapProcessor.SetHeightModifiers(heightModifiersList);

            TerrainContextMap terrainContextMap 
                = heightMapProcessor.ProcessHeight(structuralHeight, heightMapSettings, 
                    sampleCentre, step, biomeDensityMap, biomes);
            
            return terrainContextMap;
        }
    }
}