using Settings;

namespace Generators.BiomeMap
{
    public static class BiomeGeneratorFactory
    {
        public static IBiomeMapGenerator GetBiomeMapGenerator(WorldSettings worldSettings)
        {
            switch (worldSettings.biomeBlending)
            {
                case BiomeBlending.HardSeams:
                    return new BiomeMapByChunkGenerator();
                case BiomeBlending.PatchBlending:
                    return new BiomeMapWithPatchesByChunkGenerator();
                case BiomeBlending.BlobBlending:
                    return new BiomeMapWithBlobsByChunkGenerator();
                
                default:
                    return new BiomeMapByChunkGenerator();
            }
        }
    }
}