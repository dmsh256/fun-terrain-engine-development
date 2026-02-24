using UnityEngine;

namespace WorldGeneration.Biomes
{
    public class WorldBiomeGenSettings
    {
        [Header("Biome Field")] 
        public const float biomeFieldFrequency = 0.002f;
        public const float biomeWarpFrequency = 0.01f;
        public const float biomeWarpStrength = 80f;

        public const float shorelineBlend = 0.03f;
        public const float biomeContrast = 2.5f;
    }
}