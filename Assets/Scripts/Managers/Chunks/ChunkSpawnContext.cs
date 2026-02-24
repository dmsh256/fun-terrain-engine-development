using WorldGeneration.Biomes;

namespace Managers.Chunks
{
    public readonly struct ChunkSpawnContext
    {
        public readonly TerrainChunk chunk;
        public readonly IBiomeProvider biomeProvider;

        public ChunkSpawnContext(TerrainChunk chunk, IBiomeProvider biomeProvider)
        {
            this.chunk = chunk;
            this.biomeProvider = biomeProvider;
        }
    }
}