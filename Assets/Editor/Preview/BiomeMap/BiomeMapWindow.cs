using Settings.Biome;
using UnityEditor;
using UnityEngine;
using WorldGeneration.Biomes;

namespace Editor.Preview.BiomeMap
{
    public class BiomeMapWindow : EditorWindow
    {
        [Header("Biomes")] 
        public BiomeData[] biomes;

        [Header("Size")] 
        public int width = 1024;
        public int length = 1024;
        
        private Texture2D debugTexture;
        
        private Vector2 scroll;
        
        private enum ViewMode
        {
            Biomes,
            DominantBiome,
            heightMap
        }

        private ViewMode viewMode = ViewMode.Biomes;

        [MenuItem("World/Biome Debug Window")]
        public static void Open()
        {
            GetWindow<BiomeMapWindow>("Biome Debug");
        }

        private void OnGUI()
        {
            viewMode = (ViewMode)EditorGUILayout.EnumPopup("View Mode", viewMode);
            width  = EditorGUILayout.IntField("Width", width);
            length = EditorGUILayout.IntField("Length", length);

            EditorGUILayout.Space();
            GUILayout.Label("Biome Debug Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            SerializedObject so = new (this);
            SerializedProperty biomesProp = so.FindProperty("biomes");
            EditorGUILayout.PropertyField(biomesProp, true);
            so.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Debug Map"))
            {/*
                BiomeDensityMap biomeDensityMap =
                    BiomeDensityMapGenerator.BiomeDensityMapFromContext(width, length, new WorldBiomeGenGenSettings(), biomes);

                debugTexture = GenerateDebugTexture(biomeDensityMap, biomes);*/
            }

            if (debugTexture)
            {
                GUILayout.Label("Result", EditorStyles.boldLabel);
                float aspect = (float)debugTexture.width / debugTexture.height;
                Rect r = GUILayoutUtility.GetAspectRect(aspect);
                EditorGUI.DrawPreviewTexture(r, debugTexture, null, ScaleMode.ScaleToFit);
            }
        }

        public Texture2D GenerateDebugTexture(
            BiomeDensityMap biomeDensityMap,
            BiomeData[] biomeData
        )
        {
            Texture2D texture = new (biomeDensityMap.width, biomeDensityMap.height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            switch (viewMode)
            {
                case ViewMode.Biomes:
                    for (int y = 0; y < biomeDensityMap.height; y++)
                    {
                        for (int x = 0; x < biomeDensityMap.width; x++)
                        {
                            Color color = Color.magenta;
        
                            for (int i = 0; i < biomeData.Length; i++)
                            {
                                color = biomeData[biomeDensityMap.primary[x,y]].debugColor;
                            }
        
                            texture.SetPixel(x, y, color);
                        }
                    }

                    break;
                
                case ViewMode.DominantBiome:
                    for (int y = 0; y < biomeDensityMap.height; y++)
                    {
                        for (int x = 0; x < biomeDensityMap.width; x++)
                        {
                            int bestIndex = -1;
                            float bestValue = -1f;

                            for (int i = 0; i < biomes.Length; i++)
                            {
                                float d = biomeDensityMap.primary[x, y];
                                if (d > bestValue)
                                {
                                    bestValue = d;
                                    bestIndex = i;
                                }
                            }

                            Color c = bestIndex >= 0
                                ? biomes[bestIndex].debugColor
                                : Color.magenta;
                            
                            texture.SetPixel(x, y, c);
                        }
                    }
                    break;
                /*
                case ViewMode.heightMap:
                    BiomeContext[] biomeContext =
                        BiomeDensityMapGenerator.Generate(new WorldBiomeGenGenSettings(), biomes);

                    float[,] heightMap = biomeContext[0].heightMap;
                    
                    for (int y = 0; y < heightMap.GetLength(0); y++)
                    {
                        for (int x = 0; x < heightMap.GetLength(1); x++)
                        {
                            Color color = Color.Lerp (Color.black, Color.white, Mathf.InverseLerp(0, 1, heightMap[x, y]));
        
                            texture.SetPixel(x, y, color);
                        }
                    }
                    break;*/
            }
            
            texture.Apply();
            
            return texture;
        }
        
        private bool ValidateInputs()
        {
            if (biomes == null || biomes.Length == 0)
            {
                Debug.LogWarning("No biomes assigned.");
                return false;
            }

            return true;
        }
    }
}