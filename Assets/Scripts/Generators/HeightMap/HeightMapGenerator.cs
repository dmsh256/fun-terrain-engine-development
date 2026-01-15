using Generators.Noise;
using Generators.Noise.Generators;
using Generators.Noise.NoiseSource;
using Settings;
using UnityEngine;

namespace Generators.HeightMap
{
    public static class HeightMapGenerator
    {
        public static HeightMap GenerateHeightMap(int width, int length, HeightMapSettings settings, Vector2 sampleCentre)
        {
            // TODO there was an original generator once, now it's a blending engine
            
            //float[,] values = PerlinNoiseWithOctavesGenerator.GenerateNoiseMap(width, length, settings.noiseSettings, sampleCentre);
            
            INoiseSource maskNoise = NoiseGeneratorFactory.Create(settings.maskSettings);
            INoiseSource heightNoise = NoiseGeneratorFactory.Create(settings.noiseSettings);

            BlendNoiseSource blended = new (
                heightNoise,
                maskNoise,
                (h, m) => h * Mathf.SmoothStep(0.3f, 0.7f, m)
            );

            float[,] values = blended.Generate(width, length, sampleCentre);
            //AnimationCurve heightCurveThreadsafe = new (settings.heightCurve.keys);

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    values[i, j] *= /*heightCurveThreadsafe.Evaluate(values[i, j]) * */settings.heightMultiplier;

                    if (values[i, j] > maxValue)
                    {
                        maxValue = values[i, j];
                    }

                    if (values[i, j] < minValue)
                    {
                        minValue = values[i, j];
                    }
                }
            }

            return new HeightMap(values, minValue, maxValue);
        }
    }
}