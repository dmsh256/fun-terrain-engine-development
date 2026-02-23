using System;
using System.Collections.Generic;
using Generators.BiomeMap;
using Settings;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;

namespace Generators.Terrain
{
    public class TerrainGenerator : MonoBehaviour
    {
        private const float ViewerMoveThresholdForChunkUpdate = 25f;

        private const float SqrViewerMoveThresholdForChunkUpdate =
            ViewerMoveThresholdForChunkUpdate * ViewerMoveThresholdForChunkUpdate;

        public WorldSettings worldSettings;
        
        public int colliderLODIndex;
        public LODInfo[] detailLevels;
        
        [SerializeField]
        private int collisionMeshLoadRadius = 1;

        public Transform viewer;
        public Material mapMaterial;
        public Material terrainMaterial;

        public MeshSettings meshSettings; 
        public GlobalHeightMapSettings heightMapSettings;
        
        public ObjectGenerator.ObjectGenerator objectGenerator;
        
        private Vector2 viewerPosition;
        private Vector2 viewerPositionOld;

        private float meshWorldSize;
        private int chunksVisibleInViewDst;

        private readonly Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new ();
        private readonly List<TerrainChunk> visibleTerrainChunks = new ();
        
        [SerializeField]
        private LayerMask layerMask;
        
        public void Start()
        {
            GameObject fpsController = GameObject.Find("RigidBodyFPSController");
            Camera mainCamera = Camera.main;

            if (fpsController && mainCamera)
            {
                Transform head = fpsController.transform.Find("Head");

                if (head)
                {
                    mainCamera.transform.SetParent(head);
                    mainCamera.transform.localPosition = Vector3.zero;
                    mainCamera.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    Debug.LogWarning("Head object not found inside RigidBodyFPSController.");
                }
            }
            else
            {
                Debug.LogError("FPS Controller or Main Camera not found!");
            }

            Texture2DArray albedoArray =
                BiomeAlbedoArrayBuilder.Build(worldSettings.biomes);

            terrainMaterial.SetTexture("_BiomeAlbedoArray", albedoArray);
            terrainMaterial.SetInt("_BiomeCount", worldSettings.biomes.Length);

            float maxViewDistance = detailLevels[^1].visibleDstThreshold;
            meshWorldSize = meshSettings.meshWorldSize;
            chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

            objectGenerator?.Init(meshSettings);

            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }

        public void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

            if (!((viewerPositionOld - viewerPosition).sqrMagnitude > SqrViewerMoveThresholdForChunkUpdate)) 
                return;
        
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }

        private void UpdateVisibleChunks()
        {
            HashSet<Vector2> alreadyUpdatedChunkCoords = new();

            for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
            {
                alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coordinates);
                visibleTerrainChunks[i].UpdateTerrainChunk();
            }

            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);
            Vector2Int currentChunkCoord = new(currentChunkCoordX, currentChunkCoordY);

            objectGenerator?.SetCurrentChunkCoordinates(currentChunkCoord);

            for (int y = -chunksVisibleInViewDst; y <= chunksVisibleInViewDst; y++)
            {
                for (int x = -chunksVisibleInViewDst; x <= chunksVisibleInViewDst; x++)
                {
                    Vector2Int viewedChunkCoord = new(currentChunkCoordX + x, currentChunkCoordY + y);

                    if (!IsChunkCoordInsideWorld(viewedChunkCoord))
                        continue;

                    if (alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                        continue;

                    if (terrainChunkDictionary.TryGetValue(viewedChunkCoord, out TerrainChunk existingChunk))
                    {
                        existingChunk.UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new(viewedChunkCoord, worldSettings, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial, layerMask);

                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.onCollisionMeshReady += OnTerrainChunkCollisionReady;
                        newChunk.Load();
                    }
                }
            }

            UpdateCollisionChunks(currentChunkCoordX, currentChunkCoordY);
            objectGenerator?.UpdateLoadedChunks(currentChunkCoord);
        }

        private void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
        {
            if (isVisible)
                visibleTerrainChunks.Add(chunk);
            else
                visibleTerrainChunks.Remove(chunk);

            objectGenerator?.OnChunkVisibilityChanged(chunk, isVisible);
        }
        
        private void OnTerrainChunkCollisionReady(TerrainChunk chunk)
        {
            IBiomeProvider biomeProvider = new LocalBiomeMapProvider(chunk.terrainContextMap.biomeDensityMap, worldSettings.biomes);
            objectGenerator?.OnChunkCollisionReady(chunk, biomeProvider);
        }
        
        private bool IsChunkCoordInsideWorld(Vector2 coord)
        {
            int halfX = worldSettings.worldSizeInChunksX / 2;
            int halfY = worldSettings.worldSizeInChunksY / 2;

            return coord.x >= -halfX && coord.x < halfX && coord.y >= -halfY && coord.y < halfY;
        }

        private void UpdateCollisionChunks(int currentChunkCoordX, int currentChunkCoordY)
        {
            for (int y = -collisionMeshLoadRadius; y <= collisionMeshLoadRadius; y++)
            {
                for (int x = -collisionMeshLoadRadius; x <= collisionMeshLoadRadius; x++)
                {
                    Vector2Int collisionChunkCoord = new(currentChunkCoordX + x, currentChunkCoordY + y);

                    if (!IsChunkCoordInsideWorld(collisionChunkCoord))
                        continue;

                    TerrainChunk chunk = EnsureChunkExists(collisionChunkCoord);
                    chunk.RequestCollisionMesh(true);
                }
            }
        }

        private TerrainChunk EnsureChunkExists(Vector2Int chunkCoord)
        {
            if (terrainChunkDictionary.TryGetValue(chunkCoord, out TerrainChunk existingChunk))
                return existingChunk;

            TerrainChunk newChunk = new(chunkCoord, worldSettings, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial, layerMask);
            terrainChunkDictionary.Add(chunkCoord, newChunk);
            newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
            newChunk.onCollisionMeshReady += OnTerrainChunkCollisionReady;
            newChunk.Load();

            return newChunk;
        }

        public List<TerrainChunk> GetVisibleTerrainChunks()
        {
            return visibleTerrainChunks;
        }
        
        private void Awake()
        {
            WorldContext.Initialize(worldSettings);
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
