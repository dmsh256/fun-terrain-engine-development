using System.Collections.Generic;
using Generators.ObjectGenerator;
using Managers.Chunks;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Managers.Objects
{
    public class ObjectChunkManager
    {
        private readonly Dictionary<Vector2, ObjectChunk> objectChunks = new();
        private readonly Transform parentTransform;
        private readonly ChunkSpawnScheduler chunkSpawnScheduler;
        private readonly ObjectPoolManager objectPoolManager;

        public ObjectChunkManager(Transform parentTransform, ChunkSpawnScheduler chunkSpawnScheduler, ObjectPoolManager objectPoolManager)
        {
            this.parentTransform = parentTransform;
            this.chunkSpawnScheduler = chunkSpawnScheduler;
            this.objectPoolManager = objectPoolManager;
        }

        public bool IsSpawned(Vector2 coord)
        {
            return objectChunks.ContainsKey(coord);
        }

        public void Spawn(TerrainChunk terrainChunk, TerrainSpawnData terrainSpawnData, IBiomeProvider biomeProvider)
        {
            if (objectChunks.ContainsKey(terrainChunk.coordinates))
                return;

            ObjectChunk objectChunk = new (terrainSpawnData, parentTransform, biomeProvider, objectPoolManager);
            objectChunks.Add(terrainChunk.coordinates, objectChunk);

            objectChunk.PrepareSpawn();
            chunkSpawnScheduler.RegisterSpawningChunk(objectChunk);
        }
        
        public void ForEachSpawned(System.Action<Vector2> action)
        {
            foreach (Vector2 coord in objectChunks.Keys)
                action(coord);
        }

        public void Remove(Vector2 coord)
        {
            if (!objectChunks.TryGetValue(coord, out ObjectChunk objectChunk))
                return;

            objectChunk.Despawn();
            objectChunks.Remove(coord);
        }
        
        public IEnumerable<Vector2> GetSpawnedCoords()
        {
            return objectChunks.Keys;
        }

        public void Clear()
        {
            foreach (ObjectChunk objectChunk in objectChunks.Values)
                objectChunk.Despawn();

            objectChunks.Clear();
        }
    }
}