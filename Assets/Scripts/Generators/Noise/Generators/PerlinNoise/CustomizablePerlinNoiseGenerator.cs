using UnityEngine;
using WorldGeneration;
using Random = System.Random;

namespace Generators.Noise.Generators.PerlinNoise
{
    /**
     * No height normalization. Things can get really crazy ;)
     */
    public class CustomizablePerlinNoiseGenerator : INoiseGenerator
    {
        public static float[,] GenerateNoiseMap(int mapWidth, int mapLength, NoiseSettings noiseSettings, Vector2 sampleCentre)
        {
            float[,] noiseMap = new float[mapWidth, mapLength];

            int octaveCount = noiseSettings.octaveSettings.Count;
            Vector2[] octaveOffsets = new Vector2[octaveCount];

            Random randomGenerator = new(WorldContext.Seed);
            for (int i = 0; i < octaveCount; i++)
            {
                OctaveSettings octaveSettings = noiseSettings.octaveSettings[i];
                float offsetX = randomGenerator.Next(-100000, 100000) + octaveSettings.offset.x + sampleCentre.x;
                float offsetY = randomGenerator.Next(-100000, 100000) + octaveSettings.offset.y + sampleCentre.y;

                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;

            float halfWidth = mapWidth / 2f;
            float halfLength = mapLength / 2f;
            for (int y = 0; y < mapLength; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float noiseHeight = 0f;
                    for (int i = 0; i < octaveCount; i++)
                    {
                        OctaveSettings octaveSettings = noiseSettings.octaveSettings[i];

                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / octaveSettings.scale * octaveSettings.noiseFrequency;
                        float sampleY = (y - halfLength + octaveOffsets[i].y) / octaveSettings.scale * octaveSettings.noiseFrequency;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;

                        noiseHeight += perlinValue * octaveSettings.noiseAmplitude;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                        maxLocalNoiseHeight = noiseHeight;

                    if (noiseHeight < minLocalNoiseHeight)
                        minLocalNoiseHeight = noiseHeight;
                    
                    noiseMap[x, y] = noiseHeight;
                }
            }

            return noiseMap;
        }
    }
}