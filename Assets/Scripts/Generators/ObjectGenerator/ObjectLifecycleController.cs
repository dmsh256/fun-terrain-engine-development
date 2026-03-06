using System.Collections.Generic;
using Generators.BiomeMap;
using Generators.Grass;
using Managers.Chunks;
using Managers.Grass;
using Managers.Objects;
using Settings;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Generators.ObjectGenerator
{
    public class ObjectLifecycleController : MonoBehaviour
    {
        [SerializeField]
        private int chunkLoadRadius = 1;
        
        private MeshSettings meshSettings;
        private WorldSettings worldSettings;
        
        private ObjectChunkManager objectManager;
        private GrassChunkManager grassManager;
        private ChunkSpawnScheduler chunkSpawnScheduler;
        
        [SerializeField]
        private GrassIndirectRenderer grassRenderer;
        
        [SerializeField] 
        private int maxChunkSpawnsPerFrame = 1;
        [SerializeField] 
        private int maxObjectsPerFrame = 10;
        
        [Header("Fallback only, optional")]
        [SerializeField]
        public Mesh grassMesh;
        public Material grassMaterial;
        
        private Vector2Int currentChunkCoord;
        private ObjectPoolManager objectPoolManager;
        
        public void Init(MeshSettings meshSettings, WorldSettings worldSettings)
        {
            this.meshSettings = meshSettings;
            this.worldSettings = worldSettings;
            
            if (!grassRenderer)
                grassRenderer = GetComponent<GrassIndirectRenderer>();
            if (!grassRenderer)
                grassRenderer = gameObject.AddComponent<GrassIndirectRenderer>();
            
            chunkSpawnScheduler = new ChunkSpawnScheduler(maxChunkSpawnsPerFrame, maxObjectsPerFrame, SpawnFromScheduler);
            objectManager = new ObjectChunkManager(transform, chunkSpawnScheduler, new ObjectPoolManager(transform));
            grassManager = new GrassChunkManager(grassRenderer, meshSettings, grassMesh, grassMaterial);
        }
        
        private void Update()
        {
            chunkSpawnScheduler?.Update();
        }
        
        public void SetCurrentChunk(Vector2Int chunkCoord)
        {
            currentChunkCoord = chunkCoord;
        }
        
        public void OnTerrainChunkVisibilityChanged(TerrainChunk terrainChunk, bool visible)
        {
            Vector2 terrainChunkCoordinates = terrainChunk.coordinates;
            grassManager.SetVisibility(terrainChunkCoordinates, visible);
            if (!visible)
                return;

            Vector2Int chunkCoord = new((int)terrainChunkCoordinates.x, (int)terrainChunkCoordinates.y);
            if (!IsWithinRadius(chunkCoord, currentChunkCoord))
                return;

            if (objectManager.IsSpawned(terrainChunkCoordinates) || chunkSpawnScheduler.IsQueued(terrainChunkCoordinates))
                return;

            chunkSpawnScheduler.Enqueue(terrainChunk);
        }

        public void UpdateLoadedChunks(Vector2Int currentChunkCoordinates, List<TerrainChunk> visibleChunks)
        {
            List<Vector2> toRemove = new ();
            objectManager.ForEachSpawned(coord =>
            {
                Vector2Int chunkCoord = new ((int)coord.x, (int)coord.y);
                if (!IsWithinRadius(chunkCoord, currentChunkCoordinates))
                    toRemove.Add(coord);
            });

            foreach (Vector2 coord in toRemove)
            {
                grassManager.Remove(coord);
                objectManager.Remove(coord);
            }

            foreach (TerrainChunk terrainChunk in visibleChunks)
            {
                Vector2 coord = terrainChunk.coordinates;
                Vector2Int chunkCoord = new((int)coord.x, (int)coord.y);
                if (!IsWithinRadius(chunkCoord, currentChunkCoordinates))
                    continue;

                if (objectManager.IsSpawned(coord) || chunkSpawnScheduler.IsQueued(coord))
                    continue;

                chunkSpawnScheduler.Enqueue(terrainChunk);
            }
        }
        
        private bool IsWithinRadius(Vector2Int sampleChunkCoordinates, Vector2Int currentChunksCoordinates)
        {
            return Mathf.Abs(sampleChunkCoordinates.x - currentChunksCoordinates.x) <= chunkLoadRadius &&
                   Mathf.Abs(sampleChunkCoordinates.y - currentChunksCoordinates.y) <= chunkLoadRadius;
        }

        private void SpawnFromScheduler(TerrainChunk terrainChunk)
        {
            Vector2 chunkCoordinates = terrainChunk.coordinates;
            if (objectManager.IsSpawned(chunkCoordinates))
                return;

            TerrainSpawnData terrainSpawnData = new(
                new Vector2Int((int)chunkCoordinates.x, (int)chunkCoordinates.y),
                meshSettings,
                terrainChunk.terrainContextMap.heightMap,
                terrainChunk.terrainLayerMask);

            IBiomeProvider biomeProvider = new LocalBiomeMapProvider(terrainChunk.terrainContextMap.biomeDensityMap,
                worldSettings.biomes);
            
            grassManager.Spawn(terrainChunk, biomeProvider);
            objectManager.Spawn(terrainChunk, terrainSpawnData, biomeProvider);
            
            terrainChunk.ObjectsSpawned();
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
            grassManager?.Clear();
            objectManager?.Clear();
        }
    }
}
