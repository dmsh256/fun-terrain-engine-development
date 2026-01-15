using UnityEngine;

namespace Generators.Noise.Generators
{
    public class FBMNoiseGenerator : INoiseGenerator
    {
        public static float[,] GenerateNoiseMap(
            int mapWidth,
            int mapLength,
            NoiseSettings settings,
            Vector2 sampleCentre)
        {
            float[,] noiseMap = new float[mapWidth, mapLength];

            System.Random prng = new (settings.seed);
            Vector2[] octaveOffsets = new Vector2[settings.octaves];

            float amplitude = 1f;
            float maxAmplitude = 0f;

            for (int i = 0; i < settings.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
                float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxAmplitude += amplitude;
                amplitude *= settings.persistance;
            }

            float halfWidth = mapWidth / 2f;
            float halfLength = mapLength / 2f;

            for (int y = 0; y < mapLength; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float frequency = 1f;
                    amplitude = 1f;
                    float noiseHeight = 0f;

                    for (int i = 0; i < settings.octaves; i++)
                    {
                        float sampleX =
                            (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                        float sampleY =
                            (y - halfLength + octaveOffsets[i].y) / settings.scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= settings.persistance;
                        frequency *= settings.lacunarity;
                    }

                    noiseMap[x, y] = noiseHeight / maxAmplitude;
                }
            }

            return noiseMap;
        }
    }
}