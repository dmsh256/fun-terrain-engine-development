using Generators;
using Generators.HeightMap;
using Settings;
using UnityEngine;

namespace Utils.Preview
{
	public class MapPreview : MonoBehaviour 
	{
		public Renderer textureRender;
		public MeshFilter meshFilter;
	
		public enum DrawMode {NoiseMap, Mesh, FalloffMap};
		public DrawMode drawMode;

		public MeshSettings meshSettings;
		public HeightMapSettings heightMapSettings;
		public TextureData textureData;
	
		public WorldSettings worldSettings;

		public Material terrainMaterial;

		[Range(0,MeshSettings.numSupportedLODs-1)]
		public int editorPreviewLOD = 4;
	
		public bool autoUpdate;
		public bool drawMultipleChunks;

		[Header("Preview Grid")]
		public int previewChunksX = 10;
		public int previewChunksY = 10;
	
		public void DrawSingleChunkInEditor() {
			textureData.ApplyToMaterial (terrainMaterial);
			textureData.UpdateMeshHeights (terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

			var heightMap = HeightMapGenerator.GenerateHeightMap (meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);

			switch (drawMode)
			{
				case DrawMode.NoiseMap:
					DrawTexture (TextureGenerator.TextureFromHeightMap (heightMap));
					break;
				case DrawMode.Mesh:
					DrawMesh (MeshGenerator.GenerateTerrainMesh (heightMap.values, meshSettings, editorPreviewLOD));
					break;
				case DrawMode.FalloffMap:
					DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine),0,1)));
					break;
				default:
					return;
			}
		}
	
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

		private void DrawPreviewChunk(int chunkX, int chunkY)
		{
			Vector2 sampleCentre =
				new Vector2(chunkX, chunkY) *
				meshSettings.meshWorldSize /
				meshSettings.meshScale;

			GameObject chunkGO = new ($"PreviewChunk_{chunkX}_{chunkY}");
			chunkGO.transform.parent = transform;
			chunkGO.transform.localPosition = new Vector3(
				chunkX * meshSettings.meshWorldSize,
				0f,
				chunkY * meshSettings.meshWorldSize
			);

			textureData.ApplyToMaterial(terrainMaterial);
			textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

			var heightMap = HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine,
				meshSettings.numVertsPerLine,
				heightMapSettings,
				sampleCentre
			);

			switch (drawMode)
			{
				case DrawMode.NoiseMap:
					DrawTextureOnChunk(
						chunkGO,
						TextureGenerator.TextureFromHeightMap(heightMap)
					);
					break;

				case DrawMode.Mesh:
					DrawMeshOnChunk(
						chunkGO,
						MeshGenerator.GenerateTerrainMesh(
							heightMap.values,
							meshSettings,
							editorPreviewLOD
						)
					);
					break;

				case DrawMode.FalloffMap:
					DrawTextureOnChunk(
						chunkGO,
						TextureGenerator.TextureFromHeightMap(
							new HeightMap(
								FalloffGenerator.GenerateFalloffMap(
									meshSettings.numVertsPerLine
								),
								0,
								1
							)
						)
					);
					break;
			}
		}
	
		public void DrawTexture(Texture2D texture) {
			textureRender.sharedMaterial.mainTexture = texture;
			textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height) /10f;

			textureRender.gameObject.SetActive (true);
			meshFilter.gameObject.SetActive (false);
		}

		public void DrawMesh(MeshData meshData) {
			meshFilter.sharedMesh = meshData.CreateMesh ();

			textureRender.gameObject.SetActive (false);
			meshFilter.gameObject.SetActive (true);
		}

		public void OnValuesUpdated() {
			if (!Application.isPlaying) {
				DrawMultipleChunksInEditor ();
			}
		}

		public void OnTextureValuesUpdated() {
			textureData.ApplyToMaterial (terrainMaterial);
		}

		public void OnValidate() {

			if (meshSettings != null) {
				meshSettings.OnValuesUpdated -= OnValuesUpdated;
				meshSettings.OnValuesUpdated += OnValuesUpdated;
			}
		
			if (heightMapSettings != null) {
				heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
				heightMapSettings.OnValuesUpdated += OnValuesUpdated;
			}
		
			if (textureData != null) {
				textureData.OnValuesUpdated -= OnTextureValuesUpdated;
				textureData.OnValuesUpdated += OnTextureValuesUpdated;
			}
		}
	}
}
