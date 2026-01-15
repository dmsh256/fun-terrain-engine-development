using Generators.Noise;
using Generators.Noise.NoiseSource;
using Settings;
using UnityEngine;

namespace Generators.HeightMap
{
    public class GlobalHeightMapGenerator
    {
        public static HeightMap GenerateHeightMap(int width, int length, GlobalHeightMapSettings mapSettings, Vector2 sampleCentre)
        {
            //float[,] values = NoiseGeneratorFactory.Generate(width, length, mapSettings.noiseSettings, sampleCentre);
            
            INoiseSource maskNoise = NoiseGeneratorFactory.Create(mapSettings.maskSettings);
            INoiseSource heightNoise = NoiseGeneratorFactory.Create(mapSettings.noiseSettings);

            BlendNoiseSource blended = new (
                heightNoise,
                maskNoise,
                (h, m) => h * Mathf.SmoothStep(0.3f, 0.7f, m)
            );

            float[,] values = blended.Generate(width, length, sampleCentre);
            
            AnimationCurve heightCurveThreadsafe = new (mapSettings.heightCurve.keys);

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    values[i, j] *= /*heightCurveThreadsafe.Evaluate(values[i, j])*/  mapSettings.heightMultiplier;

                    maxValue = Mathf.Max(maxValue, values[i, j]);
                    minValue = Mathf.Min(minValue, values[i, j]);
                }
            }

            return new HeightMap(values, minValue, maxValue, mapSettings.heightMultiplier);
        }
    }
}