using UnityEngine;

namespace Generators.BiomeMap.BiomeTerrainShapers
{
    [CreateAssetMenu(menuName = "World/Biome Shapers/Swamp")]
    public class SwampBiomeTerrainShaper : BiomeTerrainShaper
    {
        [SerializeField] public float targetHeight = 0.01f;
        [SerializeField] public float flattenStrength = 12f;
        
        public override float Shape(TerrainContext terrainContext)
        {
            float height = Mathf.SmoothStep(terrainContext.height, targetHeight, flattenStrength * terrainContext.dominance) 
                + Mathf.PerlinNoise(terrainContext.worldX, terrainContext.worldZ) * 0.0005f;
            
            return height;
        }
    }
}