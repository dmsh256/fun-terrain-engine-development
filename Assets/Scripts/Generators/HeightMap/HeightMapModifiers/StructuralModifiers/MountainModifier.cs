using WorldGeneration.WorldStructuralModifiers;
using WorldGeneration.WorldStructuralModifiers.MountainModifier;

namespace Generators.HeightMap.HeightMapModifiers.StructuralModifiers
{
    using UnityEngine;

    public class MountainHeightModifier : IStructuralHeightModifier
    {
        private readonly MountainGenerator.PeakCluster cluster;
        private readonly float peakRadius;
        private readonly float ridgeWidth;
        
        private readonly float amplificationStrength;

        public Bounds bounds { get; }

        public MountainHeightModifier(MountainGenerator.PeakCluster cluster, float peakRadius, float ridgeWidth, float amplificationStrength)
        {
            this.cluster = cluster;
            this.peakRadius = peakRadius;
            this.ridgeWidth = ridgeWidth;
            this.amplificationStrength = amplificationStrength;
            
            bounds = CalculateBounds();
        }
        
        public float Evaluate(float worldX, float worldZ, float height)
        {
            Vector2 point = new(worldX, worldZ);

            float mask = 0f;
            foreach (MountainGenerator.Peak peak in cluster.peaks)
            {
                float distance = Vector2.Distance(point, peak.position);
                if (distance > peakRadius)
                    continue;

                float t = 1f - distance / peakRadius;
                float slopeNoise = 1f - Mathf.Abs(Mathf.PerlinNoise(worldX * 0.005f, worldZ * 0.005f));
                float slopeMod = Mathf.Lerp(0.85f, 1.15f, slopeNoise);
                t *= slopeMod;

                float contribution = t * peak.height;
                mask = mask + contribution - mask * contribution;
            }

            foreach (MountainGenerator.RidgeSpine ridge in cluster.ridges)
            {
                float distance = DistancePointToSegment(point, ridge.start, ridge.end);
                if (distance > ridgeWidth)
                    continue;

                float t = 1f - distance / ridgeWidth;
                t = Mathf.Pow(t, 4f);

                mask += t;
            }

            mask = Mathf.Clamp01(mask);
            float landFactor = Mathf.InverseLerp(0.03f, 0.03f + 200f, height);
            mask *= landFactor;
            float amplifiedHeight = height * (1f + mask * amplificationStrength);

            return amplifiedHeight;
        }
        
        private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            
            return Vector2.Distance(p, a + ab * t);
        }
        
        private Bounds CalculateBounds()
        {
            bool initialized = false;
            Bounds calculatedBounds = new();
            void Encapsulate(Vector2 p)
            {
                Vector3 v = new(p.x, 0, p.y);

                if (!initialized)
                {
                    calculatedBounds = new Bounds(v, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    calculatedBounds.Encapsulate(v);
                }
            }
            
            foreach (MountainGenerator.Peak peak in cluster.peaks)
            {
                Encapsulate(peak.position);
            }

            foreach (MountainGenerator.RidgeSpine ridge in cluster.ridges)
            {
                Encapsulate(ridge.start);
                Encapsulate(ridge.end);
            }

            calculatedBounds.Expand(Mathf.Max(peakRadius, ridgeWidth) * 2f);

            return calculatedBounds;
        }
    }
}