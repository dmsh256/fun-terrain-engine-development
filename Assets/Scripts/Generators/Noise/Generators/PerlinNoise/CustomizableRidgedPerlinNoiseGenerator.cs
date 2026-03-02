using UnityEngine;
using WorldGeneration;
using Random = System.Random;

namespace Generators.Noise.Generators.PerlinNoise
{
    public class CustomizableRidgedPerlinNoiseGenerator
    {
        public static float[,] GenerateNoiseMap(int mapWidth, int mapLength, NoiseSettings noiseSettings, Vector2 sampleCentre, float worldStep)
        {
            float[,] noiseMap = new float[mapWidth, mapLength];

            int octaveCount = noiseSettings.octaveSettings.Count;
            Vector2[] octaveOffsets = new Vector2[octaveCount];

            Random randomGenerator = new(WorldContextSettings.Seed);
            for (int i = 0; i < octaveCount; i++)
            {
                OctaveSettings octaveSettings = noiseSettings.octaveSettings[i];
                float offsetX = randomGenerator.Next(-100000, 100000) + octaveSettings.offset.x;
                float offsetY = randomGenerator.Next(-100000, 100000) + octaveSettings.offset.y;

                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;
            
            for (int y = 0; y < mapLength; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float noiseHeight = 0f;
                    float worldX = sampleCentre.x + x * worldStep;
                    float worldY = sampleCentre.y + y * worldStep;
                    for (int i = 0; i < octaveCount; i++)
                    {
                        OctaveSettings octaveSettings = noiseSettings.octaveSettings[i];

                        float sampleX = (worldX + octaveOffsets[i].x) / octaveSettings.scale * octaveSettings.noiseFrequency;
                        float sampleY = (worldY + octaveOffsets[i].y) / octaveSettings.scale * octaveSettings.noiseFrequency;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;

                        noiseHeight += perlinValue * octaveSettings.noiseAmplitude;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                        maxLocalNoiseHeight = noiseHeight;

                    if (noiseHeight < minLocalNoiseHeight)
                        minLocalNoiseHeight = noiseHeight;
                    
                    noiseMap[x, y] = 1f - Mathf.Abs(noiseHeight);
                }
            }

            return noiseMap;
        }
    }
}