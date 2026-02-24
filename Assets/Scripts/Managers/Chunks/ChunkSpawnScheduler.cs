using System.Collections.Generic;
using Generators.ObjectGenerator;
using UnityEngine;

namespace Managers.Chunks
{
    public class ChunkSpawnScheduler
    {
        private readonly Queue<TerrainChunk> spawnQueue = new();
        private readonly List<ObjectChunk> chunksSpawning = new();

        private readonly HashSet<Vector2> queuedCoords = new();
        
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
            Vector2 coord = terrainChunk.coordinates;

            if (queuedCoords.Contains(coord))
                return;

            spawnQueue.Enqueue(terrainChunk);
            queuedCoords.Add(coord);
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
                queuedCoords.Remove(terrainChunk.coordinates);
                spawnAction.Invoke(terrainChunk);
                processed++;
            }
        }
        
        public bool IsQueued(Vector2 coord)
        {
            return queuedCoords.Contains(coord);
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