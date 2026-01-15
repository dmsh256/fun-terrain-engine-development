using System;
using Generators.Noise.NoiseSource;
using UnityEngine;

namespace Generators.Noise
{
    public sealed class BlendNoiseSource : INoiseSource
    {
        private readonly INoiseSource a;
        private readonly INoiseSource b;
        private readonly Func<float, float, float> blend;

        public BlendNoiseSource(
            INoiseSource a,
            INoiseSource b,
            Func<float, float, float> blend)
        {
            this.a = a;
            this.b = b;
            this.blend = blend;
        }

        public float[,] Generate(int width, int height, Vector2 sampleCentre)
        {
            float[,] mapA = a.Generate(width, height, sampleCentre);
            float[,] mapB = b.Generate(width, height, sampleCentre);

            float[,] result = new float[width, height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    result[x, y] = blend(mapA[x, y], mapB[x, y]);

            return result;
        }
    }
}