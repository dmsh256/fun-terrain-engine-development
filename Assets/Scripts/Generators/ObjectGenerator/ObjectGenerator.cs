using System.Collections.Generic;
using Generators.BiomeMap;
using Generators.Grass;
using Managers.Chunks;
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
        private readonly List<GrassChunk> visibleGrassChunks = new();
        
        private MeshSettings meshSettings;
        private WorldSettings worldSettings;
        
        private ChunkSpawnScheduler scheduler;
        
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
            
            scheduler = new ChunkSpawnScheduler(maxChunkSpawnsPerFrame, maxObjectsPerFrame, SpawnFromScheduler);
        }
        
        private void Update()
        {
            scheduler.Update();
        }
        
        public void OnTerrainChunkVisibilityChanged(TerrainChunk terrainChunk, bool visible)
        {
            Vector2 coord = terrainChunk.coordinates;
            if (visible)
            {
                OnChunkDataReady(terrainChunk);
                if (grassChunks.TryGetValue(coord, out GrassChunk visibleGrassChunk))
                {
                    UpdateGrassChunkVisibility(visibleGrassChunk, true);
                }
                
                return;
            }
            
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

                if (!objectChunks.ContainsKey(spawnContext.chunk.coordinates))
                {
                    scheduler.Enqueue(spawnContext);
                }
            }
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

        private void SpawnFromScheduler(ChunkSpawnContext context)
        {
            if (objectChunks.ContainsKey(context.chunk.coordinates))
                return;

            SpawnObjectChunk(context.chunk, context.biomeProvider);
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
            if (terrainChunk.IsVisible())
            {
                UpdateGrassChunkVisibility(grassChunk, true);
            }
            
            ObjectChunk objectChunk = new (terrainSpawnData, transform, biomeProvider);
            objectChunks.Add(terrainChunk.coordinates, objectChunk);
            objectChunk.PrepareSpawn();
            scheduler.RegisterSpawningChunk(objectChunk);
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
        }
    }
}
