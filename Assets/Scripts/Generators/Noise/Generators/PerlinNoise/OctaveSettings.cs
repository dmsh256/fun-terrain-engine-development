using UnityEngine;

namespace Generators.Noise.Generators.PerlinNoise
{
    [CreateAssetMenu(menuName = "World/Noise/Octave Settings")]
    public class OctaveSettings : ScriptableObject
    {
        /**
         * Use at your own risk :)
         */
        
        [Header("Noise amplitude")]
        public float noiseAmplitude = 1f;
        
        [Header("Noise frequency")]
        public float noiseFrequency = 1f;
        
        public Vector2 offset;
        
        public float scale = 1f;
    }
}