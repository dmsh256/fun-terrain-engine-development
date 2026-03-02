using System.Collections.Generic;
using UnityEngine;
using WorldGeneration.WorldContextGenerator;

namespace Generators.HeightMap.HeightMapModifiers
{
    public class CanyonModifier : IHeightMapModifier
    {
        private readonly List<WorldContextGenerator.CanyonPath> paths;
        private readonly float trenchWidth;
        private readonly float waterLevel;
        
        public Bounds bounds { get; } // TODO implements decent bounds 
        
        public CanyonModifier(List<WorldContextGenerator.CanyonPath> paths, float trenchWidth, float waterLevel)
        {
            this.paths = paths;
            this.trenchWidth = trenchWidth;
            this.waterLevel = waterLevel - 0.2f;
        }

        public float Evaluate(float worldX, float worldZ, float height)
        {
            Vector2 p = new(worldX, worldZ);
            float minDist = float.MaxValue;
            for (int i = 0; i < paths.Count; i++)
            {
                float d = DistanceToPath(p, paths[i]);
                if (d < minDist)
                    minDist = d;
            }

            if (minDist >= trenchWidth)
                return height;

            float t = 1f - (minDist / trenchWidth);
            float profile = Mathf.Pow(t, 2.5f);
            if (height <= waterLevel)
                return height;

            float bowl = profile * profile;
            float target = waterLevel;
            float carved = height - (height - target) * bowl;

            return carved;
        }

        private float DistanceToPath(Vector2 p, WorldContextGenerator.CanyonPath path)
        {
            float min = float.MaxValue;
            for (int i = 0; i < path.points.Count - 1; i++)
            {
                float d = DistanceToSegment(p, path.points[i], path.points[i + 1]);
                if (d < min)
                    min = d;
            }

            return min;
        }

        private float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            Vector2 closest = a + ab * t;
            
            return Vector2.Distance(p, closest);
        }
    }
}