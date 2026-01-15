using Generators;
using Generators.HeightMap;
using Settings;
using UnityEngine;

public class TerrainChunk
{
    private const float ColliderGenerationDistanceThreshold = 5;
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord;

    private readonly GameObject meshObject;
    private readonly Vector2 sampleCentre;
    private readonly Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool heightMapReceived;
    int previousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDistance;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings,
        LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);
        
        meshObject = new GameObject("Terrain Chunk " + coord);
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].UpdateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex)
            {
                lodMeshes[i].UpdateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDistance = detailLevels[^1].visibleDstThreshold;
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(
            () => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine,
                heightMapSettings, sampleCentre), OnHeightMapReceived);
    }

    private void OnHeightMapReceived(object heightMapObject)
    {
        heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;

        UpdateTerrainChunk();
    }

    Vector2 viewerPosition => new(viewer.position.x, viewer.position.z);

    public void UpdateTerrainChunk()
    {
        if (!heightMapReceived) 
            return; // TODO what if we try to load it again?
        
        float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

        bool wasVisible = IsVisible();
        bool visible = viewerDstFromNearestEdge <= maxViewDistance;

        if (visible)
        {
            int lodIndex = 0;
            for (int i = 0; i < detailLevels.Length - 1; i++)
            {
                if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                {
                    lodIndex = i + 1;
                }
                else
                {
                    break;
                }
            }

            if (lodIndex != previousLODIndex)
            {
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;
                    meshCollider.sharedMesh = lodMesh.mesh; // TODO add mesh only for closer than X units terrain
                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(heightMap, meshSettings);
                }
            }
        }

        if (wasVisible == visible) 
            return;
        
        SetVisible(visible);
        onVisibilityChanged?.Invoke(this, visible);
    }

    public void UpdateCollisionMesh()
    {
        if (hasSetCollider) 
            return;
        
        var sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

        if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
        {
            if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
            {
                lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
            }
        }
        
        if (!(sqrDstFromViewerToEdge < ColliderGenerationDistanceThreshold * ColliderGenerationDistanceThreshold)) 
            return;

        if (!lodMeshes[colliderLODIndex].hasMesh) 
            return;
        
        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
        hasSetCollider = true;
    }

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }
    
    public Vector3 WorldPosition()
    {
        return meshObject.transform.position;
    }

    public float MeshWorldSize()
    {
        return meshSettings.meshWorldSize;
    }

    public float SampleHeight(Vector2Int localPosition)
    {
        if (!heightMapReceived) 
            return 0;
        
        return heightMap.values[localPosition.x, localPosition.y];
    }
}

internal class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    private readonly int levelOfDetail;
    public event System.Action UpdateCallback;

    public LODMesh(int levelOfDetail)
    {
        this.levelOfDetail = levelOfDetail;
    }

    private void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        UpdateCallback?.Invoke();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, levelOfDetail),
            OnMeshDataReceived);
    }
}