using UnityEngine;

namespace Generators.BiomeMap.BiomeTerrainShapers
{
    public abstract class BiomeTerrainShaper : ScriptableObject
    {
        public abstract float Shape(TerrainContext terrainContext);
    }
}