using System.Collections.Generic;
using Generators;
using Generators.HeightMap;
using Settings;
using UnityEngine;

namespace Utils.Preview
{
    public class GlobalMapPreview : MonoBehaviour
    {
		public enum DrawMode {NoiseMap, Mesh};
		public DrawMode drawMode;

		public GlobalHeightMapSettings globalHeightMapSettings;
		public MeshSettings meshSettings;
		public TextureData textureData; 
		
		private GlobalHeightMapGenerator globalHeightMapGenerator;

		public Material terrainMaterial;
		public Material waterMaterial;
		
		[Range(0,MeshSettings.numSupportedLODs-1)]
		public int editorPreviewLOD = 4;

		public bool generateObjects;
	
		public bool autoUpdate;

		[Header("Preview Grid")]
		public int previewChunksX;
		public int previewChunksY;
				
		[Header("Preview Rocks")]
		public GameObject rockPrefab;
		public int rocksPerChunk = 10;
		[Range(0f, 1f)] public float rockMinHeight = 0.1f;
		[Range(0f, 1f)] public float rockMaxHeight = 0.8f;
		
		public Vector2 rockScaleRange = new (0.9f, 1.3f);

		private int rockSeed;

		[Header("Preview Trees")]
		public GameObject treePrefab;
		public int treesPerChunk = 5;

		[Range(0f, 1f)]
		public float treeMinHeightPercent = 0.10f;
		[Range(0f, 1f)]
		public float treeMaxHeightPercent = 0.70f;

		public Vector2 treeScaleRange = new (0.9f, 1.3f);
		private int treeSeed;
		
		[Header("Preview Vegetation")]
		public GameObject vegetationPrefab;
		public int vegetationPerChunk = 5;

		[Range(0f, 1f)]
		public float vegetationMinHeightPercent = 0.10f;
		[Range(0f, 1f)]
		public float vegetationMaxHeightPercent = 0.70f;

		public Vector2 vegetationScaleRange = new (0.9f, 1.3f);
		private int vegetationSeed;
		
		private const float offsetToPlaceObjectsDeeper = 1.5f;

		private float globalMaxHeight = float.MinValue;
		private float globalMinHeight = float.MaxValue;
		
		private readonly List<ChunkContext> generatedChunks = new();
		
		public void DrawMultipleChunksInEditor() {
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(transform.GetChild(i).gameObject);
			}
			
			for (int y = 0; y < previewChunksY; y++)
			{
				for (int x = 0; x < previewChunksX; x++)
				{
					DrawPreviewChunk(x, y);
				}
			}
			
			Debug.Log ("Global maxHeight: " + globalMaxHeight + " , minHeight: " + globalMinHeight);
			
			/*GameObject hasWater = GameObject.Find("Water");
			if (hasWater)
				DestroyImmediate(hasWater);*/
			
			GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Water";
            
            water.transform.localScale = new Vector3(
                previewChunksX * meshSettings.meshWorldSize / 10f,
                1f,
                previewChunksY * meshSettings.meshWorldSize / 10f
            );

            water.transform.position = new Vector3(
                previewChunksX * meshSettings.meshWorldSize * 0.5f,
                58f, // just hardcoded for now
                previewChunksY * meshSettings.meshWorldSize * 0.5f - meshSettings.meshWorldSize
            );
            
            waterMaterial.SetFloat("_WaterHeight", 58f);
            water.GetComponent<MeshRenderer>().material = waterMaterial;
            
            textureData.ApplyToMaterial (terrainMaterial);
			textureData.UpdateMeshHeights (terrainMaterial, globalMinHeight, globalMaxHeight);

			if (generateObjects)
			{
				foreach (ChunkContext chunkContext in generatedChunks)
                {
                	DrawPreviewRocks(chunkContext.chunk, chunkContext.heightMap);
                	DrawPreviewTreesOnMesh(chunkContext.chunk, chunkContext.heightMap);
                	DrawPreviewVegetationOnMesh(chunkContext.chunk, chunkContext.heightMap);
                }
			}
		}
		
		private void DrawPreviewChunk(int chunkX, int chunkY)
		{
			Vector2 sampleCentre = new Vector2(chunkX, chunkY) * meshSettings.meshWorldSize / meshSettings.meshScale;
			
			GameObject chunk = new ($"PreviewChunk_{chunkX}_{chunkY}");
			chunk.transform.parent = transform;
			chunk.transform.localPosition = new Vector3(
				chunkX * meshSettings.meshWorldSize,
				0f,
				chunkY * meshSettings.meshWorldSize
			);
			
			HeightMap heightMap = GlobalHeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine,
				meshSettings.numVertsPerLine,
				globalHeightMapSettings,
				sampleCentre
			);

			globalMaxHeight = Mathf.Max(heightMap.maxValue, globalMaxHeight);
			globalMinHeight = Mathf.Min(heightMap.minValue, globalMinHeight);
			
			switch (drawMode)
			{
				case DrawMode.NoiseMap:
					DrawTextureOnChunk(
						chunk,
						TextureGenerator.TextureFromHeightMap(heightMap)
					);
					break;

				case DrawMode.Mesh:
					DrawMeshOnChunk(
						chunk,
						MeshGenerator.GenerateTerrainMesh(
							heightMap.values,
							meshSettings,
							editorPreviewLOD
						)
					);
					
					ChunkContext chunkContext = new(chunk, heightMap);
					generatedChunks.Add(chunkContext);
					break;
			}
		}

		private void DrawMeshOnChunk(GameObject go, MeshData meshData)
		{
			MeshFilter filter = go.GetComponent<MeshFilter>();
			MeshRenderer renderer = go.GetComponent<MeshRenderer>();

			if (!filter) 
				filter = go.AddComponent<MeshFilter>();
			
			if (!renderer) 
				renderer = go.AddComponent<MeshRenderer>();

			filter.sharedMesh = meshData.CreateMesh();
			renderer.sharedMaterial = terrainMaterial;
		}

		private void DrawTextureOnChunk(GameObject go, Texture2D texture)
		{
			MeshFilter filter = go.GetComponent<MeshFilter>();
			MeshRenderer renderer = go.GetComponent<MeshRenderer>();

			if (!filter) 
				filter = go.AddComponent<MeshFilter>();
			
			if (!renderer) 
				renderer = go.AddComponent<MeshRenderer>();

			Mesh mesh = new ();
			mesh.vertices = new[]
			{
				new Vector3(0, 0, 0),
				new Vector3(0, 0, meshSettings.meshWorldSize),
				new Vector3(meshSettings.meshWorldSize, 0, 0),
				new Vector3(meshSettings.meshWorldSize, 0, meshSettings.meshWorldSize)
			};
			
			mesh.triangles = new[] { 0, 1, 2, 2, 1, 3 };
			mesh.uv = new[]
			{
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 0),
				new Vector2(1, 1)
			};
			mesh.RecalculateNormals();

			filter.sharedMesh = mesh;

			Material mat = new (Shader.Find("Unlit/Texture"));
			mat.mainTexture = texture;
			renderer.sharedMaterial = mat;
		}
		
		private void DrawPreviewRocks(
			GameObject chunk,
			HeightMap heightMap
		)
		{
			if (!rockPrefab)
				return;

			int seed = rockSeed ^ 73856093 ^ 19349663;
			Random.InitState(seed);
			int resolution = heightMap.values.GetLength(0);

			float minH = globalMinHeight;
			float maxH = globalMaxHeight;
			float heightRange = maxH - minH;

			float rockMinWorldH = minH + heightRange * rockMinHeight;
			float rockMaxWorldH = maxH - heightRange * rockMaxHeight;

			for (int i = 0; i < rocksPerChunk; i++)
			{
				int x = Random.Range(0, resolution);
				int y = Random.Range(0, resolution);

				float worldHeight = heightMap.values[x, y];

				if (worldHeight < rockMinWorldH || worldHeight > rockMaxWorldH)
					continue;
				
				Vector3 position = new (x, worldHeight - offsetToPlaceObjectsDeeper, -y);
				
				GameObject rock = Instantiate(
					rockPrefab,
					chunk.transform
				);
				
				rock.transform.localPosition = position;
				rock.transform.localRotation =
					Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
				float scale = Random.Range(treeScaleRange.x, treeScaleRange.y);
				rock.transform.localScale = Vector3.one * scale;

				rock.name = "PreviewRock " + i;
			}
		}
		
		private void DrawPreviewTreesOnMesh(
			GameObject chunk,
			HeightMap heightMap
		)
		{
			if (!treePrefab)
				return;

			int seed = treeSeed ^ 73856093 ^ 19349663;
			Random.InitState(seed);
			int resolution = heightMap.values.GetLength(0);

			float minH = globalMinHeight;
			float maxH = globalMaxHeight;
			float heightRange = maxH - minH;

			float treeMinWorldH = minH + heightRange * treeMinHeightPercent;
			float treeMaxWorldH = maxH - heightRange * treeMaxHeightPercent;

			for (int i = 0; i < treesPerChunk; i++)
			{
				int x = Random.Range(0, resolution);
				int y = Random.Range(0, resolution);

				float worldHeight = heightMap.values[x, y];

				if (worldHeight < treeMinWorldH || worldHeight > treeMaxWorldH)
					continue;
				
				Vector3 position = new (x, worldHeight - offsetToPlaceObjectsDeeper, -y);
				
				GameObject tree = Instantiate(
					treePrefab,
					chunk.transform
				);
				
				tree.transform.localPosition = position;
				tree.transform.localRotation =
					Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
				float scale = Random.Range(treeScaleRange.x, treeScaleRange.y);
				tree.transform.localScale = Vector3.one * scale;

				tree.name = "PreviewTree " + i;
			}
		}
		
		private void DrawPreviewVegetationOnMesh(
			GameObject chunk,
			HeightMap heightMap
		)
		{
			if (!vegetationPrefab)
				return;

			int seed = vegetationSeed ^ 73856093 ^ 19349663;
			Random.InitState(seed);
			int resolution = heightMap.values.GetLength(0);

			float minH = globalMinHeight;
			float maxH = globalMaxHeight;
			float heightRange = maxH - minH;

			float treeMinWorldH = minH + heightRange * vegetationMinHeightPercent;
			float treeMaxWorldH = maxH - heightRange * vegetationMaxHeightPercent;

			for (int i = 0; i < vegetationPerChunk; i++)
			{
				int x = Random.Range(0, resolution);
				int y = Random.Range(0, resolution);

				float worldHeight = heightMap.values[x, y];

				if (worldHeight < treeMinWorldH || worldHeight > treeMaxWorldH)
					continue;
				
				Vector3 position = new (x, worldHeight - offsetToPlaceObjectsDeeper / 2f, -y);
				
				GameObject vegetation = Instantiate(
					vegetationPrefab,
					chunk.transform
				);
				
				vegetation.transform.localPosition = position;
				vegetation.transform.localRotation =
					Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
				float scale = Random.Range(treeScaleRange.x, treeScaleRange.y);
				vegetation.transform.localScale = Vector3.one * scale;

				vegetation.name = "PreviewVegetation " + i;
			}
		}
		
		public void Init()
		{
			int worldSeed = WorldManager.Instance.Seed;
			rockSeed = worldSeed;
			treeSeed = worldSeed;
			vegetationSeed = worldSeed;
		}
	}
    
    class ChunkContext
    {
	    public GameObject chunk;
	    public HeightMap heightMap;
	    
	    public ChunkContext(GameObject chunk, HeightMap heightMap)
	    {
		    this.chunk = chunk;
		    this.heightMap = heightMap;
	    }
    }
}