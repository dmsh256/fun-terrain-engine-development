namespace Generators.ObjectGenerator
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectChunk
    {
        private readonly ObjectSpawnContext context;
        private readonly List<IObjectSpawner> spawners = new();
        private readonly List<GameObject> spawnedObjects = new();

        public ObjectChunk(
            TerrainChunk terrainChunk,
            int worldSeed,
            GameObject[] treePrefabs,
            GameObject[] rockPrefabs,
            GameObject[] vegetationPrefabs,
            Transform parent
        )
        {
            context = new ObjectSpawnContext(terrainChunk, worldSeed, parent);

            spawners.Add(new TreeSpawner(treePrefabs, 200));
            spawners.Add(new RockSpawner(rockPrefabs, 100));
            spawners.Add(new VegetationSpawner(vegetationPrefabs, 200));
        }

        public void Spawn()
        {
            foreach (IObjectSpawner spawner in spawners)
            {
                spawner.Spawn(context, spawnedObjects);
            }
        }

        public void Despawn()
        {
            foreach (GameObject obj in spawnedObjects)
            {
                Object.Destroy(obj);
            }
            spawnedObjects.Clear();
        }
    }
}