using UnityEngine;

namespace Generators.HeightMap.HeightMapModifiers
{
    public class TerraceModifier : IHeightMapModifier
    {
        public Bounds bounds { get; }

        private readonly Vector2 center;
        private readonly float radius;
        private readonly float stepHeight;
        private readonly float blend;

        public TerraceModifier(Vector2 center, float radius, float stepHeight, float blend)
        {
            this.center = center;
            this.radius = radius;
            this.stepHeight = stepHeight;
            this.blend = blend;

            bounds = new Bounds(
                new Vector3(center.x, 0, center.y),
                new Vector3(radius * 2f, 0, radius * 2f)
            );
        }

        public float Evaluate(float worldX, float worldZ, float currentHeight)
        {
            float dx = worldX - center.x;
            float dz = worldZ - center.y;
            float dist = Mathf.Sqrt(dx * dx + dz * dz);

            if (dist > radius)
                return currentHeight;

            float stepped = Mathf.Floor(currentHeight / stepHeight) * stepHeight;
            
            return Mathf.Lerp(currentHeight, stepped, blend);
        }
    }
}