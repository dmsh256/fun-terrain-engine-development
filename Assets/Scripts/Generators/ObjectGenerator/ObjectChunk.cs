using Generators.Nature;
using WorldGeneration;
using WorldGeneration.Biomes;
using WorldGeneration.ObjectDistributionStrategies;

namespace Generators.ObjectGenerator
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectChunk
    {
        private readonly ObjectSpawnContext objectSpawnContext;
        private readonly List<INatureObjectSpawner> spawners = new();
        private readonly List<GameObject> spawnedObjects = new();
        private readonly List<IEnumerator<GameObject>> activeSpawnJobs = new();
        private readonly System.Random random = new(WorldContext.Seed);

        public ObjectChunk(TerrainSpawnData terrainSpawnData, Transform parent, IBiomeProvider biomeProvider)
        {
            objectSpawnContext = new ObjectSpawnContext(terrainSpawnData, parent, biomeProvider);
            spawners.Add(new TreeSpawner());
            spawners.Add(new RockSpawner());
        }

        public void PrepareSpawn()
        {
            foreach (INatureObjectSpawner spawner in spawners)
            {
                IEnumerator<GameObject> spawnJobs = spawner.Spawn(objectSpawnContext, new JitteredGridDistribution(), random.Next()).GetEnumerator();
                activeSpawnJobs.Add(spawnJobs);
            }
        }

        public int SpawnStep(int maxObjectsThisFrame)
        {
            int spawnedThisFrame = 0;

            for (int i = activeSpawnJobs.Count - 1; i >= 0; i--)
            {
                IEnumerator<GameObject> activeSpawnJob = activeSpawnJobs[i];
                while (spawnedThisFrame < maxObjectsThisFrame)
                {
                    bool hasNext = activeSpawnJob.MoveNext();
                    if (!hasNext)
                    {
                        activeSpawnJob.Dispose();
                        activeSpawnJobs.RemoveAt(i);
                        
                        break;
                    }

                    spawnedObjects.Add(activeSpawnJob.Current);
                    spawnedThisFrame++;
                }

                if (spawnedThisFrame >= maxObjectsThisFrame)
                    break;
            }

            return spawnedThisFrame;
        }
        
        public void Despawn()
        {
            foreach (IEnumerator<GameObject> job in activeSpawnJobs)
                job.Dispose();

            activeSpawnJobs.Clear();

            foreach (GameObject go in spawnedObjects)
                Object.Destroy(go);

            spawnedObjects.Clear();
        }
    }
}