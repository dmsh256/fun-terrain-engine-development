using UnityEngine;

namespace Generators.ObjectGenerator
{
    public class ObjectSpawnContext
    {
        public readonly TerrainChunk terrainChunk;
        public readonly System.Random random;
        public readonly Transform parent;

        public ObjectSpawnContext(
            TerrainChunk terrainChunk,
            int worldSeed,
            Transform parent
        )
        {
            this.terrainChunk = terrainChunk;
            this.parent = parent;

            int chunkSeed = worldSeed ^ terrainChunk.coord.GetHashCode();
            random = new System.Random(chunkSeed);
        }
    }
}