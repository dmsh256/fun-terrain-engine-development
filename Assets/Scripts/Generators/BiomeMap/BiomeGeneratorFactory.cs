using Settings;

namespace Generators.BiomeMap
{
    public static class BiomeGeneratorFactory
    {
        public static IBiomeMapGenerator GetBiomeMapGenerator(WorldSettings worldSettings)
        {
            return worldSettings.biomeBlending switch
            {
                BiomeBlending.HardSeams => new BiomeMapByChunkGenerator(),
                BiomeBlending.BlobBlending => new BiomeMapWithBlobsByChunkGenerator(),
                _ => new BiomeMapByChunkGenerator()
            };
        }
    }
}