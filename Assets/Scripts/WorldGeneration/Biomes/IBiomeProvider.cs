using Settings.Biome;
using UnityEngine;

namespace WorldGeneration.Biomes
{
    public interface IBiomeProvider
    {
        bool GetBiomeAtWorld(Vector3 worldPos, int scale, out BiomeData biome);
    }
}