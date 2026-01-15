using UnityEngine;

namespace Generators.Noise.Generators
{
    public interface INoiseGenerator
    {
        static float[,] GenerateNoiseMap(int mapWidth, int mapLength, NoiseSettings settings,
            Vector2 sampleCentre)
        {
            throw new System.NotImplementedException();
        }
    }
}