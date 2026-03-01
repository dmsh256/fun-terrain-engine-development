using Generators.Noise.Generators.PerlinNoise;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public class PerlinNoiseSource : INoiseSource
    {
        private readonly NoiseSettings noiseSettings;

        public PerlinNoiseSource(NoiseSettings noiseSettings)
        {
            this.noiseSettings = noiseSettings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre, float worldStep)
        {
            return PerlinNoiseWithOctavesGenerator.GenerateNoiseMap(
                width, height, noiseSettings, sampleCentre, worldStep
            );
        }
    }
}