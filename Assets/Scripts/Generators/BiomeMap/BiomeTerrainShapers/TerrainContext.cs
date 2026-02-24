using Settings.Biome;
using WorldGeneration.Biomes;

namespace Generators.BiomeMap.BiomeTerrainShapers
{
    public struct TerrainContext
    {
        public float worldX;
        public float worldZ;

        public float height;

        public BiomeDensityMap biomeMap;
        public BiomeData[] biomes;

        public float dominance;
        
        public int primaryBiome;
        public int secondaryBiome;
    }
}