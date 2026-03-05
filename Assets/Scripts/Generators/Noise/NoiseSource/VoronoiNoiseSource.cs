using Generators.Noise.Generators;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public class VoronoiNoiseSource : INoiseSource
    {
        private readonly NoiseSettings noiseSettings;

        public VoronoiNoiseSource(NoiseSettings noiseSettings)
        {
            this.noiseSettings = noiseSettings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre, float step, float scale)
        {
            return VoronoiNoiseGenerator.GenerateNoiseMap(
                width, height, noiseSettings, sampleCentre, step, scale
            );
        }
    }
}