using Settings;
using UnityEngine;

namespace WorldGeneration
{
    public static class WorldContext
    {
        public static int Seed { get; private set; }

        public static void Initialize(WorldSettings worldSettings)
        {
            Seed = worldSettings.useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : worldSettings.seed;

            Random.InitState(Seed);
        }
    }
}