using Generators.Noise.Generators;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public sealed class FBMNoiseSource : INoiseSource
    {
        private readonly NoiseSettings settings;

        public FBMNoiseSource(NoiseSettings settings)
        {
            this.settings = settings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre)
        {
            return FBMNoiseGenerator.GenerateNoiseMap(
                width, height, settings, sampleCentre
            );
        } 
    }
}