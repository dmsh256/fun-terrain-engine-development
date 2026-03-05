using Generators.Noise.Generators.PerlinNoise;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public class CustomizableRidgedPerlinNoiseSource : INoiseSource
    {
        private readonly NoiseSettings noiseSettings;

        public CustomizableRidgedPerlinNoiseSource(NoiseSettings noiseSettings)
        {
            this.noiseSettings = noiseSettings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre, float step, float scale = 1)
        {
            return CustomizableRidgedPerlinNoiseGenerator.GenerateNoiseMap(
                width, height, noiseSettings, sampleCentre, step, scale
            );
        }
    }
}