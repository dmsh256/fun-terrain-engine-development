using System;
using System.Collections.Generic;
using Generators.WorldBorders;
using Managers.Player;
using Managers.World;
using Settings;
using UI.LoadingScreen;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;
using WorldGeneration.WorldStructuralModifiers;

namespace Generators.Terrain
{
    public class TerrainGenerator : MonoBehaviour
    {
        private static readonly int BiomeAlbedoArray = Shader.PropertyToID("_BiomeAlbedoArray");
        private static readonly int BiomeCount = Shader.PropertyToID("_BiomeCount");
        
        public WorldSettings worldSettings;
        
        private WorldManager worldManager;
        private readonly PlayerManager playerManager = new();
        
        public int colliderLODIndex;
        public LODInfo[] detailLevels;
        
        [SerializeField]
        private int collisionMeshLoadRadius = 1;

        private const int bootstrapRadius = 1;

        public Transform viewer;
        public Material mapMaterial;
        public Material terrainMaterial;

        public MeshSettings meshSettings; 
        public HeightMapSettings heightMapSettings;
        
        public ObjectGenerator.ObjectLifecycleController objectLifecycleController;
        
        private Vector2 viewerPosition;
        private Vector2Int currentChunkCoord;

        private float meshWorldSize;
        private int chunksVisibleInViewDst;

        private readonly Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new ();
        private readonly List<TerrainChunk> visibleTerrainChunks = new ();
        private HashSet<Vector2> alreadyUpdatedChunkCoords = new();
        
        [SerializeField]
        private LayerMask layerMask;

        [SerializeField]
        public LoadingScreen loadingScreen;

        private readonly List<TerrainChunk> bootstrapChunks = new();
        private bool waitingForBootstrap = true;
        
        private WorldBorderGenerator worldBorderGenerator;
        private WorldStructure worldStructure;
        
        public void Start()
        {
            worldManager = new WorldManager(worldSettings, meshSettings);
            loadingScreen?.Show();

            WorldStructureGenerator worldStructureGenerator = new();
            worldStructure = worldStructureGenerator.Generate(
                worldSettings, heightMapSettings, meshSettings, 100); //TODO to settings
            
            Texture2DArray albedoArray =
                BiomeAlbedoArrayBuilder.Build(worldSettings.biomes);

            terrainMaterial.SetTexture(BiomeAlbedoArray, albedoArray);
            terrainMaterial.SetInt(BiomeCount, worldSettings.biomes.Length);

            float maxViewDistance = detailLevels[^1].visibleDstThreshold;
            meshWorldSize = meshSettings.meshWorldSize;
            chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

            objectLifecycleController?.Init(meshSettings, worldSettings);

            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            currentChunkCoord = worldManager.GetChunkCoord(viewerPosition);
            UpdateOrCreateVisibleChunks();

            worldBorderGenerator = new WorldBorderGenerator(worldSettings, meshSettings, heightMapSettings);
            worldBorderGenerator.CreateWorldBorders();
        }

        public void Update()
        {
            if (waitingForBootstrap)
            {
                bool allReady = true;
                foreach (TerrainChunk chunk in bootstrapChunks)
                {
                    if (!chunk.IsReadyForPlayer())
                    {
                        allReady = false;
                        UpdateOrCreateVisibleChunks();
                        break;
                    }
                }

                if (allReady)
                {
                    waitingForBootstrap = false;
                    playerManager.PlacePlayer(viewer, layerMask);
                    loadingScreen?.Hide();
                    bootstrapChunks.Clear();
                }
            }
            
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

            Vector2Int newChunkCoord = worldManager.GetChunkCoord(viewerPosition);
            if (newChunkCoord != currentChunkCoord)
            {
                currentChunkCoord = newChunkCoord;
                UpdateOrCreateVisibleChunks();
            }
        }

        private void UpdateOrCreateVisibleChunks()
        {
            alreadyUpdatedChunkCoords.Clear();

            for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
            {
                alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coordinates);
                visibleTerrainChunks[i].UpdateTerrainChunk();
            }

            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);
            
            Vector2Int chunkCoordinates = new(currentChunkCoordX, currentChunkCoordY);
            
            DoCreateOrUpdateChunks(currentChunkCoordX, currentChunkCoordY);

            UpdateCollisionChunks(currentChunkCoordX, currentChunkCoordY);
            objectLifecycleController?.UpdateLoadedChunks(chunkCoordinates, visibleTerrainChunks);
            objectLifecycleController?.SetCurrentChunk(chunkCoordinates);
        }

        private void DoCreateOrUpdateChunks(int currentChunkCoordX, int currentChunkCoordY)
        {
            List<Vector2Int> candidateCoords = new();
            for (int y = -chunksVisibleInViewDst; y <= chunksVisibleInViewDst; y++)
            {
                for (int x = -chunksVisibleInViewDst; x <= chunksVisibleInViewDst; x++)
                {
                    Vector2Int coord = new(currentChunkCoordX + x, currentChunkCoordY + y);

                    if (!worldManager.IsChunkCoordInsideWorld(coord))
                        continue;

                    if (alreadyUpdatedChunkCoords.Contains(coord))
                        continue;

                    candidateCoords.Add(coord);
                }
            }
            
            foreach (Vector2Int viewedChunkCoord in candidateCoords)
            {
                if (terrainChunkDictionary.TryGetValue(viewedChunkCoord, out TerrainChunk existingChunk))
                    existingChunk.UpdateTerrainChunk();
                else
                    CreateAndLoadNewTerrainChunk(viewedChunkCoord);
            }
        }
        
        private void CreateAndLoadNewTerrainChunk(Vector2Int viewedChunkCoord)
        {
            TerrainChunk newChunk = new(viewedChunkCoord, worldSettings, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial, layerMask);
            newChunk.SetWorldStructure(worldStructure);
            
            terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
            newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
            newChunk.LoadAsync();
            
            if (waitingForBootstrap && IsWithinBootstrapRadius(viewedChunkCoord))
            {
                bootstrapChunks.Add(newChunk);
            }
        }
        
        private void OnTerrainChunkVisibilityChanged(TerrainChunk terrainChunk, bool isVisible)
        {
            if (isVisible)
                visibleTerrainChunks.Add(terrainChunk);
            else
                visibleTerrainChunks.Remove(terrainChunk);
            
            objectLifecycleController?.OnTerrainChunkVisibilityChanged(terrainChunk, isVisible);
        }
        
        private bool IsWithinBootstrapRadius(Vector2Int coord)
        {
            return Mathf.Abs(coord.x - currentChunkCoord.x) <= bootstrapRadius &&
                   Mathf.Abs(coord.y - currentChunkCoord.y) <= bootstrapRadius;
        }

        private void UpdateCollisionChunks(int currentChunkCoordX, int currentChunkCoordY)
        {
            for (int y = -collisionMeshLoadRadius; y <= collisionMeshLoadRadius; y++)
            {
                for (int x = -collisionMeshLoadRadius; x <= collisionMeshLoadRadius; x++)
                {
                    Vector2Int collisionChunkCoord = new(currentChunkCoordX + x, currentChunkCoordY + y);
                    if (!worldManager.IsChunkCoordInsideWorld(collisionChunkCoord))
                        continue;

                    if (terrainChunkDictionary.TryGetValue(collisionChunkCoord, out TerrainChunk terrainChunk))
                    {
                        terrainChunk.RequestCollisionMesh(true);
                    }
                }
            }
        }

        public List<TerrainChunk> GetVisibleTerrainChunks()
        {
            return visibleTerrainChunks;
        }
        
        private void Awake()
        {
            WorldContextSettings.Initialize(worldSettings);
        }
    }

    [Serializable]
    public struct LODInfo
    {
        [Range(0, MeshSettings.numSupportedLODs - 1)]
        public int lod;

        public float visibleDstThreshold;

        public float sqrVisibleDstThreshold => visibleDstThreshold * visibleDstThreshold;
    }
}
