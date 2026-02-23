using Settings.Biome;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators.BiomeMap
{
    public class LocalBiomeMapProvider : IBiomeProvider
    {
        private readonly BiomeDensityMap biomeMap;
        private readonly BiomeData[] biomes;

        public LocalBiomeMapProvider(BiomeDensityMap biomeMap, BiomeData[] biomes)
        {
            this.biomes = biomes;
            this.biomeMap = biomeMap;
        }

        public bool GetBiomeAtWorld(Vector3 localPos, out BiomeData biome)
        {
            int biomeIndex = biomeMap.primary[(int)localPos.x, (int)localPos.z];
            biome = biomes[biomeIndex];
            
            return true;
        }
    }
}