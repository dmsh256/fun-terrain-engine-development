using Settings;
using UnityEngine;

namespace Generators.Noise
{
    [CreateAssetMenu(menuName = "World/Noise Settings")]
    public class NoiseSettings : ScriptableObject
    {
        public NormalizeMode normalizeMode;
        public NoiseType type = NoiseType.Perlin;

        public float scale = 50;
        public int octaves = 6;
        public float lacunarity = 2;
        
        [Range(0, 1)] public float persistance = .6f;

        public int seed;
        public Vector2 offset;
        
        public void ValidateValues()
        {
            scale = Mathf.Max(scale, 0.01f);
            octaves = Mathf.Max(octaves, 1);
            lacunarity = Mathf.Max(lacunarity, 1);
            persistance = Mathf.Clamp01(persistance);
        }
        
        public void Init()
        {
            seed = WorldManager.Instance.Seed;
        }
    }
}