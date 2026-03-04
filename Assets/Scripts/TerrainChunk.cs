using System;
using System.Collections.Generic;
using Generators;
using Generators.HeightMap;
using Generators.MeshGenerator;
using Generators.Terrain;
using Settings;
using UnityEngine;
using WorldGeneration.WorldStructuralModifiers;
using Object = UnityEngine.Object;

public class TerrainChunk
{
    private static readonly int WaterHeight = Shader.PropertyToID("_WaterHeight");
    private static readonly int BiomeCount = Shader.PropertyToID("_BiomeCount");
    public event Action<TerrainChunk, bool> onVisibilityChanged;
    
    public Vector2Int coordinates;
    private readonly Transform viewer;
    private Vector2 viewerPosition => new(viewer.position.x, viewer.position.z);

    private readonly GameObject meshObject;
    private readonly MeshFilter meshFilter;
    private readonly MeshCollider meshCollider;
    private readonly MeshSettings meshSettings;
    
    private readonly Vector2 sampleStartCoordinates;
    private readonly Bounds bounds;
    
    private readonly LODInfo[] detailLevels;
    private readonly LODMesh[] lodMeshes;
    private readonly int colliderLODIndex;
    
    private int previousLODIndex = -1;
    
    private bool collisionMeshRequested;
    private bool forceCollisionMesh;
    private readonly float maxViewDistance;
    
    private bool heightMapReceived;
    private bool biomeMapReceived;
    private bool hasSetCollider;
    private bool objectsSpawned;

    private readonly WorldSettings worldSettings;
    private readonly HeightMapSettings heightMapSettings;
    
    public TerrainContextMap terrainContextMap;
    public readonly LayerMask terrainLayerMask;

    private GameObject waterObject;
    private readonly List<IHeightMapModifier> worldHeightModifiers = new();

    private Texture2D splatMap;
    private readonly Material runtimeMaterial;
    
    private WorldStructure worldStructure;
    
    public TerrainChunk(Vector2Int coordinates, WorldSettings worldSettings, HeightMapSettings heightMapSettings, MeshSettings meshSettings,
        LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material, LayerMask terrainLayerMask)
    {
        this.coordinates = coordinates;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;
        this.worldSettings = worldSettings;
        this.terrainLayerMask = terrainLayerMask;
        
        sampleStartCoordinates = new Vector2(coordinates.x, coordinates.y) * meshSettings.meshWorldSize;
        
        Vector2 chunkWorldPosition = coordinates * meshSettings.meshWorldSize;
        Vector3 chunkCenter = new (
            chunkWorldPosition.x + meshSettings.meshWorldSize * 0.5f,
            0f,
            chunkWorldPosition.y + meshSettings.meshWorldSize * 0.5f
        );
        bounds = new Bounds(chunkCenter, new Vector3(meshSettings.meshWorldSize, 0f, meshSettings.meshWorldSize));
        
        meshObject = new GameObject("Terrain Chunk " + coordinates);
        meshObject.layer = Mathf.RoundToInt(Mathf.Log(this.terrainLayerMask.value, 2));
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        runtimeMaterial = new Material(material);
        meshRenderer.material = runtimeMaterial;

        meshObject.transform.position = new Vector3(chunkWorldPosition.x, 0f, chunkWorldPosition.y);
        meshObject.transform.parent = parent;
        SetVisible(false);
        
        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].UpdateCallback += UpdateTerrainChunk;
        }

        maxViewDistance = detailLevels[^1].visibleDstThreshold;
    }

    public void SetHeightModifier(IHeightMapModifier modifier)
    {
        worldHeightModifiers.Add(modifier);
    }
    
    public void SetWorldStructure(WorldStructure worldStructure)
    {
        this.worldStructure = worldStructure;
    }
    
    public void LoadAsync()
    {
        TerrainContextMapGenerator terrainContextMapGenerator = new(worldSettings);
        
        List<IHeightMapModifier> effectiveModifiers = new();
        foreach (IHeightMapModifier modifier in worldHeightModifiers)
        {
           if (modifier.bounds.Intersects(bounds))
                effectiveModifiers.Add(modifier);
        }
        terrainContextMapGenerator.SetHeightModifiers(effectiveModifiers);
        
        List<IStructuralHeightModifier> effectiveStructuralModifiers = new();
        foreach (IStructuralHeightModifier structuralModifier in worldStructure.structuralModifiers)
        {
            if (structuralModifier.bounds.Intersects(bounds))
                effectiveStructuralModifiers.Add(structuralModifier);
        }
        terrainContextMapGenerator.SetStructuralModifiers(effectiveStructuralModifiers);
        
        ThreadedDataRequester.RequestData(
            () => terrainContextMapGenerator.GenerateTerrainContextMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine,
                heightMapSettings, sampleStartCoordinates, worldSettings.biomes), OnTerrainContextReceived);
    }
    
    private void OnTerrainContextReceived(object terrainContextObject)
    {
        terrainContextMap = (TerrainContextMap)terrainContextObject;
        heightMapReceived = true;
        biomeMapReceived = true;
        
        GenerateSplatOnMainThread();
        
        UpdateTerrainChunk();
        TryUpdateCollisionMesh();
        
        LODMesh colliderLod = lodMeshes[colliderLODIndex];
        if (!colliderLod.hasRequestedColliderMesh)
            colliderLod.RequestColliderMesh(terrainContextMap.heightMap, meshSettings);
        
        CreateWater(worldSettings.waterMaterial);
    }
    
    private void GenerateSplatOnMainThread()
    {
        int resolution = meshSettings.numVerticesPerLine - 2;
        
        for (int i = 0; i < terrainContextMap.biomeDensityMap.splatMap.Length; i++)
        {
            Texture2D texture2D = new (resolution, resolution, TextureFormat.RGBA32, true, true);
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.filterMode = FilterMode.Bilinear;

            texture2D.SetPixels(terrainContextMap.biomeDensityMap.splatMap[i]);
            texture2D.Apply();
            
            runtimeMaterial.SetTexture("_SplatMap" + i, texture2D);
        }
        
        runtimeMaterial.SetInt(BiomeCount, worldSettings.biomes.Length);
    }
    
    public void UpdateTerrainChunk()
    {
        if (!heightMapReceived) 
            return;
        
        if (!biomeMapReceived) 
            return;

        float viewerDstFromNearestEdge = Mathf.Sqrt(
            bounds.SqrDistance(new Vector3(viewerPosition.x, 0f, viewerPosition.y))
        );

        bool wasVisible = IsVisible();
        bool visible = viewerDstFromNearestEdge <= maxViewDistance;

        if (visible)
        {
            int lodIndex = 0;
            for (int i = 0; i < detailLevels.Length - 1; i++)
            {
                if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                    lodIndex = i + 1;
                else
                    break;
            }

            if (lodIndex != previousLODIndex)
            {
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasTerrainMesh)
                {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.terrainMesh;
                }
                else if (!lodMesh.hasRequestedTerrainMesh)
                {
                    lodMesh.RequestMesh(terrainContextMap.heightMap, terrainContextMap.biomeDensityMap.primary, meshSettings);
                }
            }
        }

        if (wasVisible == visible) 
            return;
        
        SetVisible(visible);
        
        if (waterObject)
            waterObject.SetActive(visible);
        
        onVisibilityChanged?.Invoke(this, visible);
    }

    public void DebugDrawWeights(Action<Vector3, Vector3, Color> drawLine)
    {
        /*BiomeDensityMap biomeMap = terrainContextMap.biomeDensityMap;
        
        if (biomeMap.dominance == null) 
            return;

        int step = 2;
        for (int y = 0; y < biomeMap.height; y += step)
        {
            for (int x = 0; x < biomeMap.width; x += step)
            {
                float worldX = chunkWorldPosition.x + x;
                float worldZ = chunkWorldPosition.y + y;
                float worldY = terrainContextMap.heightMap.getHeight(x, y);

                Vector3 start = new (worldX, worldY, worldZ);
                int primary = biomeMap.primary[x, y];
                if (primary < 0)
                    continue;

                float dominance = biomeMap.dominance[x, y];
                float lineScale = 5f;
                Vector3 end = start + Vector3.up * dominance * lineScale;

                drawLine(start, end, worldSettings.biomes[primary].debugColor);
            }
        }*/
    }
    
    public void RequestCollisionMesh(bool force)
    {
        collisionMeshRequested = true;
        if (force)
            forceCollisionMesh = true;

        TryUpdateCollisionMesh();
    }

    private void TryUpdateCollisionMesh()
    {
        if (hasSetCollider)
            return;

        if (!collisionMeshRequested)
            return;

        if (!heightMapReceived)
            return;

        float sqrDstFromViewerToEdge = bounds.SqrDistance(new Vector3(viewerPosition.x, 0f, viewerPosition.y));

        if (!forceCollisionMesh && sqrDstFromViewerToEdge > detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
            return;

        LODMesh colliderLod = lodMeshes[colliderLODIndex];
        if (!colliderLod.hasRequestedColliderMesh)
        {
            colliderLod.RequestColliderMesh(terrainContextMap.heightMap, meshSettings);
        }

        if (!colliderLod.hasColliderMesh)
            return;

        meshCollider.cookingOptions = MeshColliderCookingOptions.None;
        meshCollider.sharedMesh = colliderLod.colliderMesh;
        hasSetCollider = true;
    }

    private void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }
    
    public void ObjectsSpawned()
    {
        objectsSpawned = true;
    }
    
    public bool IsReadyForPlayer()
    {
        if (!heightMapReceived)
            return false;
        
        if (!biomeMapReceived)
            return false;
        
        if (!objectsSpawned)
            return false;
        
        if (!hasSetCollider)
            return false;
        
        return true;
    }

    private void CreateWater(Material material)
    {
        float waterLevel = worldSettings.waterLevel * terrainContextMap.heightMap.getHeightMultiplier();
        if (terrainContextMap.heightMap.getMinHeightValue() > waterLevel)
            return;
        
        waterObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterObject.name = $"Water {coordinates}";
        waterObject.transform.SetParent(meshObject.transform, false);

        float scale = meshSettings.meshWorldSize / 10f;
        waterObject.transform.localScale = new Vector3(scale, 1f, scale);

        waterObject.transform.localPosition = new Vector3(
            meshSettings.meshWorldSize * 0.5f,
            waterLevel,
            meshSettings.meshWorldSize * 0.5f
        );

        MeshRenderer renderer = waterObject.GetComponent<MeshRenderer>();
        renderer.material = material;

        if (renderer.material.HasProperty(WaterHeight))
            renderer.material.SetFloat(WaterHeight, waterLevel);

        Object.Destroy(waterObject.GetComponent<Collider>());
    }
}

internal class LODMesh
{
    public Mesh colliderMesh;
    public Mesh terrainMesh;
    
    public bool hasRequestedTerrainMesh;
    public bool hasTerrainMesh;

    public bool hasRequestedColliderMesh;
    public bool hasColliderMesh;
    
    private readonly int levelOfDetail;
    public event Action UpdateCallback;

    public LODMesh(int levelOfDetail)
    {
        this.levelOfDetail = levelOfDetail;
    }

    private void OnMeshDataReceived(object meshDataObject)
    {
        terrainMesh = ((MeshData)meshDataObject).CreateMesh();
        hasTerrainMesh = true;

        UpdateCallback?.Invoke();
    }
    
    private void OnColliderMeshDataReceived(object meshDataObject)
    {
        colliderMesh = ((ColliderMeshData)meshDataObject).CreateColliderMesh();
        hasColliderMesh = true;
    }

    public void RequestMesh(HeightMap heightMap, int[,] biomeDensityMap, MeshSettings meshSettings)
    {
        hasRequestedTerrainMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, biomeDensityMap, meshSettings, levelOfDetail),
            OnMeshDataReceived);
    }
    
    public void RequestColliderMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedColliderMesh = true;
        ThreadedDataRequester.RequestData(() => ColliderMeshGenerator.GenerateColliderMesh(heightMap.values, meshSettings, levelOfDetail),
            OnColliderMeshDataReceived);
    }
}
