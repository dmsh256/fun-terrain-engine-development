using UnityEngine;

namespace Generators.HeightMap.HeightMapModifiers
{
    public class RadialModifier : IHeightMapModifier
    {
        private const float zeroHeight = 0f;
        
        public Bounds bounds { get; }

        private readonly Vector2 center;
        private readonly float radius;
        private readonly float strength;

        public RadialModifier(Vector2 center, float radius, float strength)
        {
            this.center = center;
            this.radius = radius;
            this.strength = strength;

            bounds = new Bounds(
                new Vector3(center.x, zeroHeight, center.y),
                new Vector3(radius * 2f, zeroHeight, radius * 2f)
            );
        }

        public float Evaluate(float worldX, float worldZ, float currentHeight)
        {
            float dx = worldX - center.x;
            float dz = worldZ - center.y;

            float distSqr = dx * dx + dz * dz;
            float radiusSqr = radius * radius;

            if (distSqr > radiusSqr)
                return currentHeight;

            float coefficient = 1f - Mathf.Sqrt(distSqr) / radius;
            coefficient = coefficient * coefficient * (3f - 2f * coefficient);

            return currentHeight + coefficient * strength;
        }
    }
}