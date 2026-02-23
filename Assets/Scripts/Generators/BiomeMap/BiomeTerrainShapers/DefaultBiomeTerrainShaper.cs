using UnityEngine;

namespace Generators.BiomeMap.BiomeTerrainShapers
{
    [CreateAssetMenu(menuName = "World/Biome Shapers/Default")]
    public class DefaultBiomeTerrainShaper : BiomeTerrainShaper
    {
        public override float Shape(TerrainContext terrainContext)
        {
            return terrainContext.height;
        }
    }
}