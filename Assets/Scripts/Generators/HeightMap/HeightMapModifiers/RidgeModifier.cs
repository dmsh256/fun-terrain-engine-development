using UnityEngine;

namespace Generators.HeightMap.HeightMapModifiers
{
    public class RidgeModifier : IHeightMapModifier
    {
        public Bounds bounds { get; }

        private readonly Vector2 origin;
        private readonly Vector2 direction;
        private readonly float width;
        private readonly float length;
        private readonly float strength;

        public RidgeModifier(Vector2 origin, Vector2 direction, float width, float length, float strength)
        {
            this.origin = origin;
            this.direction = direction.normalized;
            this.width = width;
            this.length = length;
            this.strength = strength;

            bounds = new Bounds(
                new Vector3(origin.x, 0, origin.y),
                new Vector3(length, 0, width * 2f)
            );
        }

        public float Evaluate(float worldX, float worldZ, float currentHeight)
        {
            Vector2 point = new (worldX, worldZ);
            Vector2 toPoint = point - origin;

            float along = Vector2.Dot(toPoint, direction);
            if (along < 0f || along > length)
                return currentHeight;

            Vector2 perp = toPoint - direction * along;
            float dist = perp.magnitude;

            if (dist > width)
                return currentHeight;

            float t = 1f - dist / width;
            t = t * t * (3f - 2f * t);

            return currentHeight + t * strength;
        }
    }
}