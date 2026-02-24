using System.Collections.Generic;
using Generators.BiomeMap;
using Generators.Grass;
using Settings;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;
using WorldGeneration.ObjectDistributionStrategies;

namespace Generators.ObjectGenerator
{
    public class ObjectGenerator : MonoBehaviour
    {
        [SerializeField]
        private int chunkLoadRadius = 1;

        private readonly Dictionary<Vector2, ObjectChunk> objectChunks = new();
        private readonly Dictionary<Vector2, GrassChunk> grassChunks = new();
        private readonly Dictionary<Vector2, ChunkSpawnContext> dataReadyChunks = new();
        private readonly HashSet<Vector2> visibleGrassChunkCoords = new();
        private readonly List<GrassChunk> visibleGrassChunks = new();
        private readonly Queue<ChunkSpawnContext> spawnQueue = new();
        private readonly List<ObjectChunk> chunksSpawning = new();
        
        private MeshSettings meshSettings;
        private WorldSettings worldSettings;
        
        [SerializeField]
        private GrassIndirectRenderer grassRenderer;
        private Vector2Int currentChunkCoordinates;
        
        [SerializeField] 
        private int maxChunkSpawnsPerFrame = 1;
        [SerializeField] 
        private int maxObjectsPerFrame = 10;
        
        [Header("Fallback only, optional")]
        [SerializeField]
        public Mesh grassMesh;
        public Material grassMaterial;
        
        public void Init(MeshSettings meshSettings, WorldSettings worldSettings)
        {
            this.meshSettings = meshSettings;
            this.worldSettings = worldSettings;
            
            if (!grassRenderer)
                grassRenderer = GetComponent<GrassIndirectRenderer>();
            if (!grassRenderer)
                grassRenderer = gameObject.AddComponent<GrassIndirectRenderer>();
        }

        private void Update()
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

                if (objectChunks.ContainsKey(context.chunk.coordinates))
                    continue;

                SpawnObjectChunk(context.chunk, context.biomeProvider);
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
        
        public void OnTerrainChunkVisibilityChanged(TerrainChunk terrainChunk, bool visible)
        {
            Vector2 coord = terrainChunk.coordinates;
            if (visible)
            {
                OnChunkDataReady(terrainChunk);
                visibleGrassChunkCoords.Add(coord);
                if (grassChunks.TryGetValue(coord, out GrassChunk visibleGrassChunk))
                {
                    UpdateGrassChunkVisibility(visibleGrassChunk, true);
                }
                
                return;
            }
            
            visibleGrassChunkCoords.Remove(coord);
            if (grassChunks.TryGetValue(coord, out GrassChunk hiddenGrassChunk))
            {
                hiddenGrassChunk.Release();
                UpdateGrassChunkVisibility(hiddenGrassChunk, false);
                grassChunks.Remove(coord);
            }

            if (objectChunks.TryGetValue(coord, out ObjectChunk chunk))
            {
                chunk.Despawn();
                objectChunks.Remove(coord);
                dataReadyChunks.Remove(coord);
            }
        }

        private void OnChunkDataReady(TerrainChunk terrainChunk)
        {
            IBiomeProvider biomeProvider = new LocalBiomeMapProvider(terrainChunk.terrainContextMap.biomeDensityMap, worldSettings.biomes);
            dataReadyChunks[terrainChunk.coordinates] = new ChunkSpawnContext(terrainChunk, biomeProvider);
            if (IsWithinRadius(new Vector2Int(terrainChunk.coordinates.x, terrainChunk.coordinates.y), currentChunkCoordinates))
            {
                SpawnObjectChunk(terrainChunk, biomeProvider);
            }
        }

        public void UpdateLoadedChunks(Vector2Int currentChunkCoordinates)
        {
            SetCurrentChunkCoordinates(currentChunkCoordinates);

            List<Vector2> chunksToDespawn = new();
            foreach (Vector2 chunkCoord in objectChunks.Keys)
            {
                Vector2Int coord = new((int)chunkCoord.x, (int)chunkCoord.y);
                if (!IsWithinRadius(coord, this.currentChunkCoordinates))
                {
                    chunksToDespawn.Add(chunkCoord);
                }
            }

            foreach (Vector2 chunkCoord in chunksToDespawn)
            {
                if (grassChunks.TryGetValue(chunkCoord, out GrassChunk grassChunk))
                {
                    grassChunk.Release();
                    UpdateGrassChunkVisibility(grassChunk, false);
                }

                visibleGrassChunkCoords.Remove(chunkCoord);
                objectChunks[chunkCoord].Despawn();
                objectChunks.Remove(chunkCoord);
                grassChunks.Remove(chunkCoord);
            }

            foreach (ChunkSpawnContext spawnContext in dataReadyChunks.Values)
            {
                Vector2Int chunkCoordinates = new(spawnContext.chunk.coordinates.x, spawnContext.chunk.coordinates.y);
                if (!IsWithinRadius(chunkCoordinates, this.currentChunkCoordinates))
                    continue;

                if (objectChunks.ContainsKey(spawnContext.chunk.coordinates))
                    continue;

                if (!objectChunks.ContainsKey(spawnContext.chunk.coordinates) &&
                    !IsAlreadyQueued(spawnContext.chunk.coordinates))
                {
                    spawnQueue.Enqueue(spawnContext);
                }
            }
        }
        
        private bool IsAlreadyQueued(Vector2 coordinates) // TODO doubtful, requires review
        {
            foreach (ChunkSpawnContext chunkSpawnContext in spawnQueue)
            {
                if (chunkSpawnContext.chunk.coordinates == coordinates)
                    return true;
            }
            return false;
        }

        public void SetCurrentChunkCoordinates(Vector2Int chunkCoordinates)
        {
            currentChunkCoordinates = chunkCoordinates;
        }

        private bool IsWithinRadius(Vector2Int sampleChunkCoordinates, Vector2Int currentChunksCoordinates)
        {
            return Mathf.Abs(sampleChunkCoordinates.x - currentChunksCoordinates.x) <= chunkLoadRadius &&
                   Mathf.Abs(sampleChunkCoordinates.y - currentChunksCoordinates.y) <= chunkLoadRadius;
        }

        private void SpawnObjectChunk(TerrainChunk terrainChunk, IBiomeProvider biomeProvider)
        {
            if (objectChunks.ContainsKey(terrainChunk.coordinates))
                return;
            
            if (grassChunks.ContainsKey(terrainChunk.coordinates))
                return;
            
            TerrainSpawnData terrainSpawnData = new (
                new Vector2Int(terrainChunk.coordinates.x, terrainChunk.coordinates.y),
                meshSettings,
                terrainChunk.terrainContextMap.heightMap,
                terrainChunk.terrainLayerMask
            );
            
            GrassChunk grassChunk = GrassChunkGenerator.Generate(
                terrainSpawnData,
                biomeProvider,
                new JitteredGridDistribution(),
                WorldContext.Seed,
                fallbackMesh: grassMesh,
                fallbackMaterial: grassMaterial);
            grassChunks.Add(terrainChunk.coordinates, grassChunk);
            if (visibleGrassChunkCoords.Contains(terrainChunk.coordinates))
            {
                UpdateGrassChunkVisibility(grassChunk, true);
            }
            
            ObjectChunk objectChunk = new (terrainSpawnData, transform, biomeProvider);
            objectChunks.Add(terrainChunk.coordinates, objectChunk);
            objectChunk.PrepareSpawn();
            chunksSpawning.Add(objectChunk);
        }

        private void UpdateGrassChunkVisibility(GrassChunk grassChunk, bool visible)
        {
            if (visible)
            {
                if (!visibleGrassChunks.Contains(grassChunk))
                    visibleGrassChunks.Add(grassChunk);

                grassRenderer?.SetVisibleChunks(visibleGrassChunks);
                grassChunk.BuildBuffers();
                return;
            }

            visibleGrassChunks.Remove(grassChunk);
            grassRenderer?.SetVisibleChunks(visibleGrassChunks);
        }

        private readonly struct ChunkSpawnContext
        {
            public readonly TerrainChunk chunk;
            public readonly IBiomeProvider biomeProvider;

            public ChunkSpawnContext(TerrainChunk chunk, IBiomeProvider biomeProvider)
            {
                this.chunk = chunk;
                this.biomeProvider = biomeProvider;
            }
        }
        
        private void OnDisable()
        {
            CleanupAllChunks();
        }

        private void OnDestroy()
        {
            CleanupAllChunks();
        }

        private void CleanupAllChunks()
        {
            foreach (GrassChunk grassChunk in grassChunks.Values)
                grassChunk.Release();

            grassChunks.Clear();
            foreach (ObjectChunk objectChunk in objectChunks.Values)
                objectChunk.Despawn();

            objectChunks.Clear();

            visibleGrassChunks.Clear();
            visibleGrassChunkCoords.Clear();
            spawnQueue.Clear();
            chunksSpawning.Clear();
        }
    }
}
