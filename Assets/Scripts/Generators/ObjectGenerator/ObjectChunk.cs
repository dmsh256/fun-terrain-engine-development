using Generators.Nature;
using Managers.Objects;
using WorldGeneration;
using WorldGeneration.Biomes;
using WorldGeneration.ObjectDistributionStrategies;
using System.Collections.Generic;
using UnityEngine;

namespace Generators.ObjectGenerator
{
    public class ObjectChunk
    {
        private readonly ObjectSpawnContext objectSpawnContext;
        private readonly List<INatureObjectSpawner> spawners = new();
        private readonly List<GameObject> spawnedObjects = new();
        private readonly System.Random random = new(WorldContextSettings.Seed);
        private readonly Queue<GameObject> pendingObjects = new();
        private readonly IObjectDistributionStrategy distributionStrategy = new PoissonDiskDistribution();
        private readonly System.Action<GameObject> emitAction;

        public ObjectChunk(TerrainSpawnData terrainSpawnData, Transform parent, IBiomeProvider biomeProvider, 
            ObjectPoolManager objectPoolManager)
        {
            emitAction = Emit;
            
            objectSpawnContext = new ObjectSpawnContext(terrainSpawnData, parent, biomeProvider, objectPoolManager);
            spawners.Add(new TreeSpawner());
            spawners.Add(new RockSpawner());
        }

        public void PrepareSpawn()
        {
            foreach (INatureObjectSpawner spawner in spawners)
            {
                spawner.Spawn(objectSpawnContext, distributionStrategy, random.Next(), emitAction);
            }
        }

        private void Emit(GameObject gameObject)
        {
            pendingObjects.Enqueue(gameObject);
        }
        
        public int SpawnGradually(int maxObjectsThisFrame)
        {
            int spawned = 0;
            while (spawned < maxObjectsThisFrame && pendingObjects.Count > 0)
            {
                GameObject gameObject = pendingObjects.Dequeue();
                spawnedObjects.Add(gameObject);
                spawned++;
            }

            return spawned;
        }
        
        public void Despawn()
        {
            foreach (GameObject gameObject in spawnedObjects)
            {
                if (!gameObject)
                    continue;

                PooledObject pooled = gameObject.GetComponent<PooledObject>();
                if (pooled && pooled.Prefab)
                    objectSpawnContext.objectPoolManager.Despawn(gameObject, pooled.Prefab);
                else
                    Object.Destroy(gameObject);
            }
            
            while (pendingObjects.Count > 0)
            {
                GameObject gameObject = pendingObjects.Dequeue();
                if (!gameObject) 
                    continue;

                PooledObject pooled = gameObject.GetComponent<PooledObject>();
                if (pooled && pooled.Prefab)
                    objectSpawnContext.objectPoolManager.Despawn(gameObject, pooled.Prefab);
                else
                    Object.Destroy(gameObject);
            }

            spawnedObjects.Clear();
        }
    }
}