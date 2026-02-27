using Generators.HeightMap;
using Settings;
using Settings.Biome;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Utils.Preview.BiomeMapTextureGenerator
{
    [ExecuteAlways]
    public class GlobalBiomeMapPreview : MonoBehaviour
    {
         [Header("Debug")] 
         public bool regenerate = true;
        
        [Header("World Settings")] [Range(1, 100)]
        public int chunksX = 10;
        [Range(1, 100)] 
        public int chunksZ = 10;
        [Range(50, 245)] 
        public int chunkSize = 245;

        [Header("Biome Settings")] 
        public BiomeData[] biomes;
        
        [Header("HeightMap Settings")]
        public HeightMapSettings heightMapSettings;
        
        public MeshSettings meshSettings;
        
        private HeightMap heightMap;
        private Texture2D biomeTexture;

        private void Update()
        {
            if (!regenerate) return;

            regenerate = false;
            Generate();
        }

        public void Generate()
        {/* TODO rewrite using new architecture
            int worldWidth = chunksX * chunkSize;
            int worldHeight = chunksZ * chunkSize;

            heightMap = GlobalHeightMapPreviewGenerator.GenerateGlobalHeightMap(chunksX,chunksZ, chunkSize, meshSettings, heightMapSettings);
            
            BiomeDensityMap biomeDensityMap =
                BiomeDensityMapGenerator.BiomeDensityMapFromContext(
                    worldHeight,
                    worldWidth,
                    biomes,
                    heightMap
                );

            biomeTexture = BiomeMapToTexture(biomeDensityMap);

            ApplyTexture(biomeTexture);
            ResizePreviewMesh(worldWidth, worldHeight);*/
        }

        private Texture2D BiomeMapToTexture(BiomeDensityMap biomeDensityMap)
        {
            Texture2D texture = new (
                biomeDensityMap.width,
                biomeDensityMap.height,
                TextureFormat.RGBA32,
                false
            );
            
            for (int y = 0; y < biomeDensityMap.height; y++)
            {
                for (int x = 0; x < biomeDensityMap.width; x++)
                {
                    int biomeIndex = biomeDensityMap.primary[x, y];
                    Color color = biomeIndex == -1 ? new Color(255f, 0f, 255f, 1f) : biomes[biomeIndex].debugColor;

                    texture.SetPixel(x, y, color);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();

            return texture;
        }

        private void ApplyTexture(Texture2D texture)
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (!meshRenderer)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Material mat = meshRenderer.sharedMaterial;

            if (!mat || mat.shader.name != "Unlit/Texture")
            {
                mat = new Material(Shader.Find("Unlit/Texture"));
                meshRenderer.sharedMaterial = mat;
            }

            mat.mainTexture = texture;
        }

        private void ResizePreviewMesh(int width, int height)
        {
            transform.localScale = new Vector3(
                width / 10f,
                1f,
                height / 10f
            );
        }
    }
}