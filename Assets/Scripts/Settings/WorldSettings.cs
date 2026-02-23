using Settings.Biome;

namespace Settings
{
    using UnityEngine;

    [CreateAssetMenu(
        fileName = "WorldSettings",
        menuName = "World/World settings"
    )]
    public class WorldSettings : UpdatableData
    {
        [Header("Seed")]
        public int seed;
        public bool useRandomSeed;

        public string worldName;
        
        [Header("Size in chunks")]
        public int worldSizeInChunksX;
        public int worldSizeInChunksY;
        
        [Header("Materials")]
        public Material borderMaterial; // TODO move somewhere
        public Material waterMaterial;

        [Header("Biomes")] 
        public BiomeData[] biomes;
        
        [Header("Biome blending")]
        public BiomeBlending biomeBlending;
        
        [Header("Water level")]
        public float waterLevel = 50f;
    }
    
    public enum BiomeBlending {
        HardSeams,
        PatchBlending,
        BlobBlending,
    }
}