using Generators.Noise.NoiseSource;

namespace Generators.Noise
{
    public static class NoiseGeneratorFactory
    {
        public static INoiseSource Create(NoiseSettings noiseSettings)
        {
            return noiseSettings.type switch
            {
                NoiseType.FBM => new FBMNoiseSource(noiseSettings),
                NoiseType.Simplex => new SimplexNoiseSource(noiseSettings),
                NoiseType.Voronoi => new VoronoiNoiseSource(noiseSettings),
                NoiseType.CustomizablePerlin => new CustomizablePerlinNoiseSource(noiseSettings),
                NoiseType.CustomizableRidgedPerlin => new CustomizableRidgedPerlinNoiseSource(noiseSettings),
                _ =>  new PerlinNoiseSource(noiseSettings)
            };
        }
    }
}