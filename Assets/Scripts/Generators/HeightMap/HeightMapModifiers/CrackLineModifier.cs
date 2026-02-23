using UnityEngine;

namespace Generators.HeightMap.HeightMapModifiers
{
    public class CrackLineModifier : IHeightMapModifier
    {
        public Bounds bounds { get; }

        private readonly Vector2 origin;
        private readonly Vector2 direction;
        private readonly float length;
        private readonly float halfWidth;
        private readonly float depth;

        public CrackLineModifier(Vector2 origin, Vector2 direction, float length, float width, float depth)
        {
            this.origin = origin;
            this.direction = direction.normalized;
            this.length = length;
            halfWidth = width * 0.5f;
            this.depth = depth;

            bounds = new Bounds(
                new Vector3(origin.x, 0f, origin.y),
                new Vector3(length, 0f, width * 4f)
            );
        }

        public float Evaluate(float worldX, float worldZ, float currentHeight)
        {
            Vector2 p = new (worldX, worldZ);
            Vector2 toPoint = p - origin;

            float along = Vector2.Dot(toPoint, direction);

            if (along < 0f || along > length)
                return currentHeight;

            Vector2 perpendicular = toPoint - direction * along;
            
            float warp = Mathf.PerlinNoise(worldX * 0.1f, worldZ * 0.1f) - 0.5f;
            perpendicular += new Vector2(-direction.y, direction.x) * warp * 3f;
            float dist = perpendicular.magnitude;

            if (dist > halfWidth)
                return currentHeight;

            float t = 1f - dist / halfWidth;
            t = t * t * (3f - 2f * t);

            float result = currentHeight - t * depth;

            return Mathf.Clamp01(result);
        }
    }
}