using UnityEngine;

namespace Generators.HeightMap.HeightMapModifiers
{
    public class FlattenModifier : IHeightMapModifier
    {
        public Bounds bounds { get; }

        private readonly Vector2 center;
        private readonly float radius;
        private readonly float targetHeight;
        private readonly float falloff;

        public FlattenModifier(Vector2 center, float radius, float targetHeight, float falloff = 2f)
        {
            this.center = center;
            this.radius = radius;
            this.targetHeight = targetHeight;
            this.falloff = falloff;

            bounds = new Bounds(
                new Vector3(center.x, 0f, center.y),
                new Vector3(radius * 2f, 0f, radius * 2f)
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

            float dist = Mathf.Sqrt(distSqr);
            float t = 1f - dist / radius;

            t = Mathf.Pow(t, falloff);

            return Mathf.Lerp(currentHeight, targetHeight, t);
        }
    }
}