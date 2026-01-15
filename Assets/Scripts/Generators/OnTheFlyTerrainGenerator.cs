using System.Collections.Generic;
using Generators.ObjectGenerator;
using Settings;
using UnityEngine;

namespace Generators
{
    public class OnTheFlyTerrainGenerator : MonoBehaviour
    {
        private const float ViewerMoveThresholdForChunkUpdate = 25f;

        private const float SqrViewerMoveThresholdForChunkUpdate =
            ViewerMoveThresholdForChunkUpdate * ViewerMoveThresholdForChunkUpdate;

        public int colliderLODIndex;
        public LODInfo[] detailLevels;

        public Transform viewer;
        public Material mapMaterial;

        public MeshSettings meshSettings; 
        public HeightMapSettings heightMapSettings;
        public TextureData textureSettings;
        public OnTheFlyObjectGenerator objectGenerator;
        
        private Vector2 viewerPosition;
        private Vector2 viewerPositionOld;

        private float meshWorldSize;
        private int chunksVisibleInViewDst;

        private readonly Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new ();
        private readonly List<TerrainChunk> visibleTerrainChunks = new ();

        public void Start()
        {
            //meshSettings = ScriptableObject.CreateInstance<MeshSettings>(); 
            //heightMapSettings = ScriptableObject.CreateInstance<HeightMapSettings>();
            //textureSettings = ScriptableObject.CreateInstance<TextureData>();
            //mapMaterial = new Material(Shader.Find("Custom/Terrain"));
        
            GameObject fpsController = GameObject.Find("RigidBodyFPSController"); // Find the FPS controller
            Camera mainCamera = Camera.main;

            if (fpsController != null && mainCamera != null)
            {
                Transform head = fpsController.transform.Find("Head"); // Adjust if needed

                if (head != null)
                {
                    mainCamera.transform.SetParent(head); // Attach the camera to the "Head"
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

            textureSettings.ApplyToMaterial(mapMaterial);
            textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

            float maxViewDistance = detailLevels[^1].visibleDstThreshold;
            meshWorldSize = meshSettings.meshWorldSize;
            chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

            UpdateVisibleChunks();
        }

        public void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

            if (viewerPosition != viewerPositionOld)
            {
                foreach (TerrainChunk chunk in visibleTerrainChunks)
                    chunk.UpdateCollisionMesh();
            }

            if (!((viewerPositionOld - viewerPosition).sqrMagnitude > SqrViewerMoveThresholdForChunkUpdate)) 
                return;
        
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }

        private void UpdateVisibleChunks()
        {
            HashSet<Vector2> alreadyUpdatedChunkCoords = new ();
            for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
            {
                alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
                visibleTerrainChunks[i].UpdateTerrainChunk();
            }

            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

            for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
            {
                for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
                {
                    Vector2 viewedChunkCoord = new (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                    if (alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) 
                        continue;
                
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new (viewedChunkCoord, heightMapSettings, meshSettings,
                            detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
                        
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }
            }
        }

        private void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
        {
            if (isVisible)
                visibleTerrainChunks.Add(chunk);
            else
                visibleTerrainChunks.Remove(chunk);
            
            objectGenerator?.OnChunkVisibilityChanged(chunk, isVisible);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0, MeshSettings.numSupportedLODs - 1)]
        public int lod;

        public float visibleDstThreshold;

        public float sqrVisibleDstThreshold => visibleDstThreshold * visibleDstThreshold;
    }
}