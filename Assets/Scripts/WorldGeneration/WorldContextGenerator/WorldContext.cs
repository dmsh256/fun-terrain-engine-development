using System.Collections.Generic;
using Generators.HeightMap;
using WorldGeneration.Biomes;

namespace WorldGeneration.WorldContextGenerator
{
    public class WorldContext
    {
        public HeightMap globalScaledHeightMap;
        public BiomeDensityMap globalScaledBiomeMap;
        public float resolution;
        public float worldStep;

        public List<WorldContextGenerator.LakeData> lakes;
        public List<WorldContextGenerator.LakeConnection> connections;
        public List<WorldContextGenerator.CanyonPath> CanyonPaths;
    }
}