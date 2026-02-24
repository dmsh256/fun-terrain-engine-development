using System.Collections.Generic;
using Generators.ObjectGenerator;

namespace Managers.Chunks
{
    public class ChunkSpawnScheduler
    {
        private readonly Queue<ChunkSpawnContext> spawnQueue = new();
        private readonly List<ObjectChunk> chunksSpawning = new();

        private readonly int maxChunkSpawnsPerFrame;
        private readonly int maxObjectsPerFrame;

        private readonly System.Action<ChunkSpawnContext> spawnAction;

        public ChunkSpawnScheduler(int maxChunkSpawnsPerFrame, int maxObjectsPerFrame, System.Action<ChunkSpawnContext> spawnAction)
        {
            this.maxChunkSpawnsPerFrame = maxChunkSpawnsPerFrame;
            this.maxObjectsPerFrame = maxObjectsPerFrame;
            this.spawnAction = spawnAction;
        }

        public void Enqueue(ChunkSpawnContext context)
        {
            spawnQueue.Enqueue(context);
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
                ChunkSpawnContext context = spawnQueue.Dequeue();
                spawnAction.Invoke(context);
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