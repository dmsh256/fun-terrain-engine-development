using Generators.Noise.Generators.PerlinNoise;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public class CustomizablePerlinNoiseSource : INoiseSource
    {
        private readonly NoiseSettings noiseSettings;

        public CustomizablePerlinNoiseSource(NoiseSettings noiseSettings)
        {
            this.noiseSettings = noiseSettings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre, float worldStep)
        {
            return CustomizablePerlinNoiseGenerator.GenerateNoiseMap(
                width, height, noiseSettings, sampleCentre, worldStep
            );
        }
    }
}