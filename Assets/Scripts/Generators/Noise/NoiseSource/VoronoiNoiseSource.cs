using Generators.Noise.Generators;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public class VoronoiNoiseSource : INoiseSource
    {
        private readonly NoiseSettings settings;

        public VoronoiNoiseSource(NoiseSettings settings)
        {
            this.settings = settings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre)
        {
            return VoronoiNoiseGenerator.GenerateNoiseMap(
                width, height, settings, sampleCentre
            );
        }
    }
}