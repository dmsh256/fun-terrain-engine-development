using System;
using UnityEngine;

namespace Settings.Biome
{
    [Serializable]
    public struct BiomeGrassVariant
    {
        public Mesh mesh;
        public Material material;
        
        [Min(0f)]
        public float weight;
        
        [Min(0.01f)]
        public float scale;
        
        [Range(-5f, 5f)] 
        public float yOffset;
    }
}
