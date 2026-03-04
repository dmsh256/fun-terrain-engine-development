using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators
{
    public struct TerrainContextMap
    {
        public HeightMap.HeightMap heightMap;
        public readonly BiomeDensityMap biomeDensityMap;
        public readonly Vector2 sampledFrom;
        public readonly float sampledWithStep;

        public TerrainContextMap(HeightMap.HeightMap heightMap, BiomeDensityMap biomeDensityMap, Vector2 sampledFrom, float sampledWithStep)
        {
            this.heightMap = heightMap;
            this.biomeDensityMap = biomeDensityMap;
            this.sampledFrom = sampledFrom;
            this.sampledWithStep = sampledWithStep;
        }
    }
}