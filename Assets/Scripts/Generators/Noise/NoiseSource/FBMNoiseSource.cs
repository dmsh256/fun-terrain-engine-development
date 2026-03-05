using Generators.Noise.Generators;
using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public sealed class FBMNoiseSource : INoiseSource
    {
        private readonly NoiseSettings noiseSettings;

        public FBMNoiseSource(NoiseSettings noiseSettings)
        {
            this.noiseSettings = noiseSettings;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre, float step, float scale)
        {
            return FBMNoiseGenerator.GenerateNoiseMap(
                width, height, noiseSettings, sampleCentre, step, scale
            );
        } 
    }
}