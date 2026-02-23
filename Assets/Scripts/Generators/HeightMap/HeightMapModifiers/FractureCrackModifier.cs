using System.Collections.Generic;
using UnityEngine;

namespace Generators.HeightMap.HeightMapModifiers
{
    public class FractureCrackModifier : IHeightMapModifier
    {
        public Bounds bounds { get; }

        private readonly List<Vector2> points = new();
        private readonly float width;
        private readonly float depth;

        public FractureCrackModifier(Vector2 origin, float length, float stepSize, float width, float depth, int seed)
        {
            this.width = width;
            this.depth = depth;

            GeneratePath(origin, length, stepSize, seed);

            bounds = ComputeBounds();
        }

        private void GeneratePath(Vector2 origin, float length, float stepSize, int seed)
        {
            Vector2 dir = RandomDirection(seed);
            Vector2 current = origin;

            points.Add(current);
            float traveled = 0f;
            while (traveled < length)
            {
                float noise = Mathf.PerlinNoise(current.x * 0.05f + seed, current.y * 0.05f + seed) - 0.5f;

                float angle = noise * 30f;
                dir = Rotate(dir, angle);

                current += dir.normalized * stepSize;
                points.Add(current);

                traveled += stepSize;
            }
        }

        private Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        private Vector2 RandomDirection(int seed)
        {
            float angle = Mathf.Abs(Mathf.Sin(seed * 12.9898f) * 43758.5453f);
            angle -= Mathf.Floor(angle);
            angle *= Mathf.PI * 2f;

            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        
        private Bounds ComputeBounds()
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            foreach (Vector2 p in points)
            {
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minZ) minZ = p.y;
                if (p.y > maxZ) maxZ = p.y;
            }

            minX -= width;
            maxX += width;
            minZ -= width;
            maxZ += width;

            Vector3 center = new ((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
            Vector3 size = new (maxX - minX, 0f, maxZ - minZ);

            return new Bounds(center, size);
        }
        
        private float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            Vector2 closest = a + ab * t;
            
            return Vector2.Distance(p, closest);
        }
        
        public float Evaluate(float worldX, float worldZ, float currentHeight)
        {
            if (!bounds.Contains(new Vector3(worldX, 0f, worldZ)))
                return currentHeight;

            Vector2 p = new(worldX, worldZ);

            float minDist = float.MaxValue;

            for (int i = 0; i < points.Count - 1; i++)
            {
                float d = DistanceToSegment(p, points[i], points[i + 1]);
                if (d < minDist)
                    minDist = d;
            }

            if (minDist > width)
                return currentHeight;

            float t = 1f - minDist / width;
            t = t * t * (3f - 2f * t);
            float result = currentHeight - t * depth;

            return Mathf.Clamp01(result);
        }
    }
}