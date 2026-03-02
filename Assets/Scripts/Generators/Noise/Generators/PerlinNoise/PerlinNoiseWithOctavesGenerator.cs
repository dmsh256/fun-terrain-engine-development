using UnityEngine;
using WorldGeneration;
using Random = System.Random;

namespace Generators.Noise.Generators.PerlinNoise
{
    public class PerlinNoiseWithOctavesGenerator
    {
        public static float[,] GenerateNoiseMap(int mapWidth, int mapLength, NoiseSettings noiseSettings, Vector2 sampleCentre, float worldStep)
        {
            float[,] noiseMap = new float[mapWidth, mapLength];

            Random randomGenerator = new (WorldContextSettings.Seed);
            Vector2[] octaveOffsets = new Vector2[noiseSettings.octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;

            for (int i = 0; i < noiseSettings.octaves; i++)
            {
                float offsetX = randomGenerator.Next(-100000, 100000) + noiseSettings.offset.x;
                float offsetY = randomGenerator.Next(-100000, 100000) + noiseSettings.offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= noiseSettings.noiseAmplitude;
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;
            
            for (int y = 0; y < mapLength; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    float worldX = sampleCentre.x + x * worldStep;
                    float worldY = sampleCentre.y + y * worldStep;
                    for (int i = 0; i < noiseSettings.octaves; i++)
                    {
                        float sampleX = (worldX + octaveOffsets[i].x) / noiseSettings.scale * frequency;
                        float sampleY = (worldY + octaveOffsets[i].y) / noiseSettings.scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= noiseSettings.noiseAmplitude;
                        frequency *= noiseSettings.noiseFrequency;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                        maxLocalNoiseHeight = noiseHeight;

                    if (noiseHeight < minLocalNoiseHeight)
                        minLocalNoiseHeight = noiseHeight;

                    noiseMap[x, y] = noiseHeight;
                    
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }

            return noiseMap;
        }
    }
}