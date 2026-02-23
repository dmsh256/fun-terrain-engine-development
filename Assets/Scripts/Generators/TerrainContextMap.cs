using WorldGeneration.Biomes;

namespace Generators
{
    public struct TerrainContextMap
    {
        public HeightMap.HeightMap heightMap;
        public readonly BiomeDensityMap biomeDensityMap;

        public TerrainContextMap(HeightMap.HeightMap heightMap, BiomeDensityMap biomeDensityMap)
        {
            this.heightMap = heightMap;
            this.biomeDensityMap = biomeDensityMap;
        }
    }
}