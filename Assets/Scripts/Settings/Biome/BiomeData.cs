using Generators.BiomeMap.BiomeTerrainShapers;
using Settings.Prefabs;
using UnityEngine;

namespace Settings.Biome
{
    [CreateAssetMenu(menuName = "World/Biome Data")]
    public class BiomeData : ScriptableObject
    {
        public string biomeName;
        public Color debugColor;
        
        [Header("Weight")]
        [Range(0f, 2f)]
        public float globalWeight = 1f;
        
        [Header("Min and max height available for biome")]
        public float minHeight;
        public float maxHeight;
        
        public BiomeTerrainShaper terrainShaper;
        
        [Header("Terrain")]
        public Texture2D soilTexture;
        public Texture2D grassTexture;
        public Texture2D rockTexture;

        [Header("Object Spawning")] 
        public bool hasTrees;
        public SpawnablePrefab[] trees;

        public bool hasRocks;
        public SpawnablePrefab[] rocks;
        
        [Header("Grass")]
        public bool hasGrass;
        public float grassDensity;
        public BiomeGrassVariant[] grassVariants;
    }
}
