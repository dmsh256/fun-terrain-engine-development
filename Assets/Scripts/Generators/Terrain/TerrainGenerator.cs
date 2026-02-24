using System;
using System.Collections.Generic;
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
            Texture2DArray albedoArray =
                BiomeAlbedoArrayBuilder.Build(worldSettings.biomes);

            terrainMaterial.SetTexture("_BiomeAlbedoArray", albedoArray);
            terrainMaterial.SetInt("_BiomeCount", worldSettings.biomes.Length);

            float maxViewDistance = detailLevels[^1].visibleDstThreshold;
            meshWorldSize = meshSettings.meshWorldSize;
            chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

            objectGenerator?.Init(meshSettings, worldSettings);

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
                        existingChunk.UpdateTerrainChunk();
                    else
                        CreateNewTerrainChunk(viewedChunkCoord);
                }
            }

            UpdateCollisionChunks(currentChunkCoordX, currentChunkCoordY);
            objectGenerator?.UpdateLoadedChunks(currentChunkCoord);
        }

        private void CreateNewTerrainChunk(Vector2Int viewedChunkCoord)
        {
            TerrainChunk newChunk = new(viewedChunkCoord, worldSettings, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial, layerMask);

            terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
            newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
            newChunk.Load();
        }

        private void OnTerrainChunkVisibilityChanged(TerrainChunk terrainChunk, bool isVisible)
        {
            if (isVisible)
                visibleTerrainChunks.Add(terrainChunk);
            else
                visibleTerrainChunks.Remove(terrainChunk);
            
            objectGenerator?.OnTerrainChunkVisibilityChanged(terrainChunk, isVisible);
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
