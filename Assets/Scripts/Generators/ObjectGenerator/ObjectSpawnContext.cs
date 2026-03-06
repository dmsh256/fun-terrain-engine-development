using Managers.Objects;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators.ObjectGenerator
{
    public class ObjectSpawnContext
    {
        public readonly TerrainSpawnData terrainSpawnData;
        public readonly Transform parent;
        public readonly IBiomeProvider biomeProvider;
        
        public readonly ObjectPoolManager objectPoolManager;

        public ObjectSpawnContext(TerrainSpawnData terrainSpawnData, Transform parent, IBiomeProvider biomeProvider,
            ObjectPoolManager objectPoolManager)
        {
            this.terrainSpawnData = terrainSpawnData;
            this.parent = parent;
            this.biomeProvider = biomeProvider;
            this.objectPoolManager = objectPoolManager;
        }
    }
}