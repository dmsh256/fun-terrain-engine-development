using UnityEngine;

namespace Generators.Noise
{
    [System.Serializable]
    public class NoiseLayer
    {
        public NoiseSettings settings;

        public bool enabled = true;

        public float weight = 1f;

        public bool useAsMask;

        public Vector2 maskSmoothRange = new (0.3f, 0.7f);
        
        public NoiseBlendMode blendMode = NoiseBlendMode.Add;
        
        public bool invertInput;
    }
}