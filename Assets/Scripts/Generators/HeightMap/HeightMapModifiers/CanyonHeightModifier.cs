using UnityEngine;
using WorldGeneration.WorldStructuralModifiers;
using WorldGeneration.WorldStructuralModifiers.LakeConnectionsModifier;

namespace Generators.HeightMap.HeightMapModifiers
{
    public class CanyonHeightModifier : IStructuralHeightModifier
    {
        private readonly LakeConnectionGenerator.CanyonPath canyonPath;
        private readonly float trenchWidthSqr;
        private readonly float waterLevel;
        
        public Bounds bounds { get; }
        
        public CanyonHeightModifier(LakeConnectionGenerator.CanyonPath canyonPath, float trenchWidth, float waterLevel)
        {
            this.canyonPath = canyonPath;
            trenchWidthSqr = trenchWidth * trenchWidth;
            this.waterLevel = waterLevel - 0.1f;

            bounds = this.canyonPath.bounds;
        }

        public float Evaluate(float worldX, float worldZ, float height)
        {
            if (!canyonPath.bounds.Contains(new Vector3(worldX, 0f, worldZ)))
                return height;
            
            Vector2 point = new(worldX, worldZ);
            float minDistSqr = DistanceToPathSqr(point, canyonPath);

            if (minDistSqr >= trenchWidthSqr)
                return height;

            float normalized = 1f - minDistSqr / trenchWidthSqr;
            float profile = Mathf.Pow(normalized, 1.25f);

            if (height <= waterLevel)
                return height;

            float bowl = profile * profile;

            return height - (height - waterLevel) * bowl;
        }

        private float DistanceToPathSqr(Vector2 point, LakeConnectionGenerator.CanyonPath path)
        {
            float minDistance = float.MaxValue;
            int segmentCount = path.points.Count - 1;
            for (int i = 0; i < segmentCount; i++)
            {
                float distance = DistanceToSegmentSqr(point, path.points[i], path.points[i + 1]);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        private float DistanceToSegmentSqr(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            Vector2 closest = a + ab * t;
            
            return (point - closest).sqrMagnitude;
        }
    }
}