using Generators.Noise.Generators;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public sealed class SimplexNoiseSource : INoiseSource
    {
        private readonly NoiseSettings settings;

        public SimplexNoiseSource(NoiseSettings settings)
        {
            this.settings = settings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre)
        {
            return SimplexNoiseGenerator.GenerateNoiseMap(
                width, height, settings, sampleCentre
            );
        }
    }
}