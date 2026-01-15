using Generators.Noise.Generators;
using Generators.Noise.NoiseSource;
using UnityEngine;

namespace Generators.Noise
{
    public static class NoiseGeneratorFactory
    {
        public static float[,] Generate(
            int width,
            int height,
            NoiseSettings settings,
            Vector2 sampleCentre)
        {
            return settings.type switch
            {
                NoiseType.FBM => FBMNoiseGenerator.GenerateNoiseMap(width, height, settings, sampleCentre),
                NoiseType.Simplex => SimplexNoiseGenerator.GenerateNoiseMap(width, height, settings, sampleCentre),
                NoiseType.Voronoi => VoronoiNoiseGenerator.GenerateNoiseMap(width, height, settings, sampleCentre),
                _ => PerlinNoiseWithOctavesGenerator.GenerateNoiseMap(width, height, settings, sampleCentre)
            };
        }
        
        public static INoiseSource Create(NoiseSettings settings)
        {
            return settings.type switch
            {
                NoiseType.FBM => new FBMNoiseSource(settings),
                NoiseType.Simplex => new SimplexNoiseSource(settings),
                NoiseType.Voronoi => new VoronoiNoiseSource(settings),
                _ => new PerlinNoiseSource(settings)
            };
        }
    }
}