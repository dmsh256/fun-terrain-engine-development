using Generators.Noise.Generators;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public sealed class SimplexNoiseSource : INoiseSource
    {
        private readonly NoiseSettings noiseSettings;

        public SimplexNoiseSource(NoiseSettings noiseSettings)
        {
            this.noiseSettings = noiseSettings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre, float step)
        {
            return SimplexNoiseGenerator.GenerateNoiseMap(
                width, height, noiseSettings, sampleCentre, step
            );
        }
    }
}