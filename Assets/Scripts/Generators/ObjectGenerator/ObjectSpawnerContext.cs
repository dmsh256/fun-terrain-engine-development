using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators.ObjectGenerator
{
    public class ObjectSpawnContext
    {
        public readonly TerrainSpawnData terrainSpawnData;
        public readonly Transform parent;
        public readonly IBiomeProvider biomeProvider;

        public ObjectSpawnContext(TerrainSpawnData terrainSpawnData, Transform parent, IBiomeProvider biomeProvider)
        {
            this.terrainSpawnData = terrainSpawnData;
            this.parent = parent;
            this.biomeProvider = biomeProvider;
        }
    }
}