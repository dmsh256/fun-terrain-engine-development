using System.Collections.Generic;
using Generators.Noise.Generators.PerlinNoise;
using UnityEngine;

namespace Generators.Noise
{
    [CreateAssetMenu(menuName = "World/Noise Settings")]
    public class NoiseSettings : ScriptableObject
    {
        public NoiseType type = NoiseType.Perlin;

        public float scale = 50;
        public int octaves = 6;
        
        [Header("Noise frequency")]
        public float noiseFrequency = 2;
        
        [Header("Noise amplitude")]
        [Range(0, 1)] 
        public float noiseAmplitude = .6f;

        public Vector2 offset;
        
        public void ValidateValues()
        {
            scale = Mathf.Max(scale, 0.01f);
            octaves = Mathf.Max(octaves, 1);
            noiseFrequency = Mathf.Max(noiseFrequency, 1);
            noiseAmplitude = Mathf.Clamp01(noiseAmplitude);
        }
        
        [Header("(!) Only for customizable perlin noise")]
        public List<OctaveSettings> octaveSettings;
    }
}