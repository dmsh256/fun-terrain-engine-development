using Generators.Noise.Generators.PerlinNoise;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public class CustomizablePerlinNoiseSource : INoiseSource
    {
        private readonly NoiseSettings settings;

        public CustomizablePerlinNoiseSource(NoiseSettings settings)
        {
            this.settings = settings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre)
        {
            return CustomizablePerlinNoiseGenerator.GenerateNoiseMap(
                width, height, settings, sampleCentre
            );
        }
    }
}