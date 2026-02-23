using UnityEngine;

namespace Generators.Noise.NoiseSource
{
    public interface INoiseSource
    {
        float[,] Generate(int width, int height, Vector2 sampleCentre);
    }
}