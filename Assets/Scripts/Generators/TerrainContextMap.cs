using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators
{
    public struct TerrainContextMap
    {
        public HeightMap.HeightMap heightMap;
        public readonly BiomeDensityMap biomeDensityMap;
        public readonly Vector2 sampledFrom;

        public TerrainContextMap(HeightMap.HeightMap heightMap, BiomeDensityMap biomeDensityMap, Vector2 sampledFrom)
        {
            this.heightMap = heightMap;
            this.biomeDensityMap = biomeDensityMap;
            this.sampledFrom = sampledFrom;
        }
    }
}