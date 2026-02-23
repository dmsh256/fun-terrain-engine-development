using Generators.Noise.Generators.PerlinNoise;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public class CustomizableRidgedPerlinNoiseSource : INoiseSource
    {
        private readonly NoiseSettings settings;

        public CustomizableRidgedPerlinNoiseSource(NoiseSettings settings)
        {
            this.settings = settings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre)
        {
            return CustomizableRidgedPerlinNoiseGenerator.GenerateNoiseMap(
                width, height, settings, sampleCentre
            );
        }
    }
}