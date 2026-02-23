using UnityEngine;

namespace Generators.HeightMap.HeightMapModifiers
{
    public class NoiseWarpModifier : IHeightMapModifier
    {
        public Bounds bounds { get; }

        private readonly Vector2 center;
        private readonly float radius;
        private readonly float frequency;
        private readonly float amplitude;

        public NoiseWarpModifier(Vector2 center, float radius, float frequency, float amplitude)
        {
            this.center = center;
            this.radius = radius;
            this.frequency = frequency;
            this.amplitude = amplitude;

            bounds = new Bounds(
                new Vector3(center.x, 0, center.y),
                new Vector3(radius * 2f, 0, radius * 2f)
            );
        }

        public float Evaluate(float worldX, float worldZ, float currentHeight)
        {
            float dx = worldX - center.x;
            float dz = worldZ - center.y;

            if (dx * dx + dz * dz > radius * radius)
                return currentHeight;

            float noise = Mathf.PerlinNoise(worldX * frequency, worldZ * frequency);
            
            return currentHeight + (noise - 0.5f) * 2f * amplitude;
        }
    }
}