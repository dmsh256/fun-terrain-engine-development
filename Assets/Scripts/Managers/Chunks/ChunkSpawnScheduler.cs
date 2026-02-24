using System.Collections.Generic;
using Generators.ObjectGenerator;

namespace Managers.Chunks
{
    public class ChunkSpawnScheduler
    {
        private readonly Queue<TerrainChunk> spawnQueue = new();
        private readonly List<ObjectChunk> chunksSpawning = new();

        private readonly int maxChunkSpawnsPerFrame;
        private readonly int maxObjectsPerFrame;

        private readonly System.Action<TerrainChunk> spawnAction;

        public ChunkSpawnScheduler(int maxChunkSpawnsPerFrame, int maxObjectsPerFrame, System.Action<TerrainChunk> spawnAction)
        {
            this.maxChunkSpawnsPerFrame = maxChunkSpawnsPerFrame;
            this.maxObjectsPerFrame = maxObjectsPerFrame;
            this.spawnAction = spawnAction;
        }

        public void Enqueue(TerrainChunk terrainChunk)
        {
            spawnQueue.Enqueue(terrainChunk);
        }

        public void RegisterSpawningChunk(ObjectChunk chunk)
        {
            chunksSpawning.Add(chunk);
        }

        public void Update()
        {
            ProcessSpawnQueue();
            ProcessObjectSpawning();
        }

        private void ProcessSpawnQueue()
        {
            int processed = 0;

            while (spawnQueue.Count > 0 && processed < maxChunkSpawnsPerFrame)
            {
                TerrainChunk terrainChunk = spawnQueue.Dequeue();
                spawnAction.Invoke(terrainChunk);
                processed++;
            }
        }

        private void ProcessObjectSpawning()
        {
            int remainingBudget = maxObjectsPerFrame;

            for (int i = chunksSpawning.Count - 1; i >= 0; i--)
            {
                if (remainingBudget <= 0)
                    break;

                ObjectChunk chunk = chunksSpawning[i];
                int spawned = chunk.SpawnStep(remainingBudget);

                remainingBudget -= spawned;

                if (spawned == 0)
                    chunksSpawning.RemoveAt(i);
            }
        }
    }
}