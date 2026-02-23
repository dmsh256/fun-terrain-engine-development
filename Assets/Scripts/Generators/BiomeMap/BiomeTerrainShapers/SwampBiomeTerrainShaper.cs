using UnityEngine;

namespace Generators.BiomeMap.BiomeTerrainShapers
{
    [CreateAssetMenu(menuName = "World/Biome Shapers/Swamp")]
    public class SwampBiomeTerrainShaper : BiomeTerrainShaper
    {
        [SerializeField] public float targetHeight = 0.01f;
        [SerializeField] public float dominanceThreshold = 0.003f;
        [SerializeField] public float flattenStrength = 12f;
        
        public override float Shape(TerrainContext terrainContext)
        {
            float dominance = terrainContext.dominance;

            if (dominance < dominanceThreshold)
                return terrainContext.height;
            
            float height = Mathf.SmoothStep(terrainContext.height, targetHeight, dominance * flattenStrength) 
                + Mathf.PerlinNoise(terrainContext.worldX, terrainContext.worldZ) * 0.0005f;
            
            return height;
        }
    }
}