using Generators.Noise.Generators;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public class PerlinNoiseSource : INoiseSource
    {
        private readonly NoiseSettings settings;

        public PerlinNoiseSource(NoiseSettings settings)
        {
            this.settings = settings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre)
        {
            return PerlinNoiseWithOctavesGenerator.GenerateNoiseMap(
                width, height, settings, sampleCentre
            );
        }
    }
}