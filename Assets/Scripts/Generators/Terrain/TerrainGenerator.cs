using System;
using System.Collections.Generic;
using Settings;
using UI.LoadingScreen;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;

namespace Generators.Terrain
{
    public class TerrainGenerator : MonoBehaviour
    {
        private static readonly int BiomeAlbedoArray = Shader.PropertyToID("_BiomeAlbedoArray");
        private static readonly int BiomeCount = Shader.PropertyToID("_BiomeCount");
        
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
        
        public ObjectGenerator.ObjectLifecycleController objectLifecycleController;
        
        private Vector2 viewerPosition;
        private Vector2Int currentChunkCoord;

        private float meshWorldSize;
        private int chunksVisibleInViewDst;

        private readonly Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new ();
        private readonly List<TerrainChunk> visibleTerrainChunks = new ();
        
        [SerializeField]
        private LayerMask layerMask;

        [SerializeField]
        public LoadingScreen loadingScreen;
        private bool isInitialLoading = true;
        
        public void Start()
        {
            loadingScreen?.Show();
            
            Texture2DArray albedoArray =
                BiomeAlbedoArrayBuilder.Build(worldSettings.biomes);

            terrainMaterial.SetTexture(BiomeAlbedoArray, albedoArray);
            terrainMaterial.SetInt(BiomeCount, worldSettings.biomes.Length);

            float maxViewDistance = detailLevels[^1].visibleDstThreshold;
            meshWorldSize = meshSettings.meshWorldSize;
            chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

            objectLifecycleController?.Init(meshSettings, worldSettings);

            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            currentChunkCoord = GetChunkCoord(viewerPosition);
            UpdateOrCreateVisibleChunks();
        }

        public void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

            Vector2Int newChunkCoord = GetChunkCoord(viewerPosition);
            if (newChunkCoord != currentChunkCoord)
            {
                currentChunkCoord = newChunkCoord;
                UpdateOrCreateVisibleChunks();
            }
        }

        private void UpdateOrCreateVisibleChunks()
        {
            HashSet<Vector2> alreadyUpdatedChunkCoords = new();

            for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
            {
                alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coordinates);
                visibleTerrainChunks[i].UpdateTerrainChunk();
            }

            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);
            
            Vector2Int chunkCoordinates = new(currentChunkCoordX, currentChunkCoordY);
            
            DoCreateOrUpdateChunks(currentChunkCoordX, currentChunkCoordY, alreadyUpdatedChunkCoords);

            UpdateCollisionChunks(currentChunkCoordX, currentChunkCoordY);
            objectLifecycleController?.UpdateLoadedChunks(chunkCoordinates, visibleTerrainChunks);
            objectLifecycleController?.SetCurrentChunk(chunkCoordinates);
        }

        private void DoCreateOrUpdateChunks(int currentChunkCoordX, int currentChunkCoordY, HashSet<Vector2> alreadyUpdatedChunkCoords)
        {
            if (isInitialLoading)
            {
                LoadInitialChunksSync();
            }
            
            List<Vector2Int> candidateCoords = new();
            for (int y = -chunksVisibleInViewDst; y <= chunksVisibleInViewDst; y++)
            {
                for (int x = -chunksVisibleInViewDst; x <= chunksVisibleInViewDst; x++)
                {
                    Vector2Int coord = new(currentChunkCoordX + x, currentChunkCoordY + y);

                    if (!IsChunkCoordInsideWorld(coord))
                        continue;

                    if (alreadyUpdatedChunkCoords.Contains(coord))
                        continue;

                    candidateCoords.Add(coord);
                }
            }
            
            candidateCoords.Sort((a, b) =>
            {
                int dxA = a.x - currentChunkCoordX;
                int dyA = a.y - currentChunkCoordY;
                int dxB = b.x - currentChunkCoordX;
                int dyB = b.y - currentChunkCoordY;

                int distA = dxA * dxA + dyA * dyA;
                int distB = dxB * dxB + dyB * dyB;

                return distA.CompareTo(distB);
            });
            
            foreach (Vector2Int viewedChunkCoord in candidateCoords)
            {
                if (terrainChunkDictionary.TryGetValue(viewedChunkCoord, out TerrainChunk existingChunk))
                    existingChunk.UpdateTerrainChunk();
                else
                    CreateAndLoadNewTerrainChunk(viewedChunkCoord);
            }
        }

        private void LoadInitialChunksSync()
        {
            List<Vector2Int> initialCoords = new()
            {
                currentChunkCoord,
                currentChunkCoord - Vector2Int.right,
                currentChunkCoord - Vector2Int.up,
                currentChunkCoord - Vector2Int.one
            };

            foreach (Vector2Int initialCoord in initialCoords)
            {
                CreateAndLoadNewTerrainChunkSync(initialCoord);
            }

            PlacePlayer();
                
            loadingScreen?.Hide();
            isInitialLoading = false;
        }

        private void PlacePlayer()
        {
            Rigidbody rigidBody = viewer.GetComponentInParent<Rigidbody>();
            Collider collider = rigidBody.GetComponent<Collider>();
            collider.enabled = false;

            Ray ray = new (viewer.position + Vector3.up * 100f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
            {
                rigidBody.MovePosition(
                    new Vector3(rigidBody.position.x, hit.point.y + 1.5f, rigidBody.position.z)
                );
            }
            
            collider.enabled = true;
        }
        
        private void CreateAndLoadNewTerrainChunk(Vector2Int viewedChunkCoord)
        {
            TerrainChunk newChunk = new(viewedChunkCoord, worldSettings, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial, layerMask);

            terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
            newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
            newChunk.LoadAsync();
        }
        
        private void OnTerrainChunkVisibilityChanged(TerrainChunk terrainChunk, bool isVisible)
        {
            if (isVisible)
                visibleTerrainChunks.Add(terrainChunk);
            else
                visibleTerrainChunks.Remove(terrainChunk);
            
            objectLifecycleController?.OnTerrainChunkVisibilityChanged(terrainChunk, isVisible);
        }
        
        private void CreateAndLoadNewTerrainChunkSync(Vector2Int viewedChunkCoord)
        {
            TerrainChunk newChunk = new(viewedChunkCoord, worldSettings, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial, layerMask);

            terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
            newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
            newChunk.LoadSync();
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
        
        private Vector2Int GetChunkCoord(Vector2 position)
        {
            int x = Mathf.RoundToInt(position.x / meshWorldSize);
            int y = Mathf.RoundToInt(position.y / meshWorldSize);
            return new Vector2Int(x, y);
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
