using UnityEngine;

namespace Generators.HeightMap.HeightMapModifiers
{  
    /**
     * This is experimental, use with small numbers
     */
    public class BunCrackModifier : IHeightMapModifier 
    {
        public Bounds bounds { get; }

        private readonly float cellSize;
        private readonly float crackWidth;
        private readonly float crackDepth;
        private readonly float cellHeightVariation;
        private readonly int seed;

        public BunCrackModifier(Bounds bounds, float cellSize, float crackWidth, float crackDepth, 
            float cellHeightVariation, int seed)
        {
            this.bounds = bounds;
            this.cellSize = cellSize;
            this.crackWidth = crackWidth;
            this.crackDepth = crackDepth;
            this.cellHeightVariation = cellHeightVariation;
            this.seed = seed;
        }

        public float Evaluate(float worldX, float worldZ, float currentHeight)
        {
            if (!bounds.Contains(new Vector3(worldX, 0f, worldZ)))
                return currentHeight;

            Vector2 p = new (worldX, worldZ);

            int gx = Mathf.FloorToInt(worldX / cellSize);
            int gz = Mathf.FloorToInt(worldZ / cellSize);

            float minDist = float.MaxValue;
            float secondMinDist = float.MaxValue;
            Vector2 closestCenter = Vector2.zero;

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector2 cell = new (gx + x, gz + z);
                    Vector2 center = cell * cellSize + RandomOffset(cell);

                    float dist = (p - center).sqrMagnitude;

                    if (dist < minDist)
                    {
                        secondMinDist = minDist;
                        minDist = dist;
                        closestCenter = center;
                    }
                    else if (dist < secondMinDist)
                    {
                        secondMinDist = dist;
                    }
                }
            }

            float edgeFactor = Mathf.Sqrt(secondMinDist) - Mathf.Sqrt(minDist);

            float crack = Mathf.Clamp01(1f - edgeFactor / crackWidth);
            float crackCarve = crack * crackDepth;
            float cellVariation = Hash(closestCenter) * cellHeightVariation;

            return currentHeight + cellVariation - crackCarve;
        }

        private Vector2 RandomOffset(Vector2 cell)
        {
            float rx = Hash(cell + Vector2.one * 17.123f);
            float rz = Hash(cell + Vector2.one * 43.456f);
            
            return new Vector2(rx, rz) * cellSize;
        }

        private float Hash(Vector2 p)
        {
            float h = Mathf.Sin(Vector2.Dot(p, new Vector2(127.1f, 311.7f)) + seed) * 43758.5453f;
            
            return h - Mathf.Floor(h);
        }
    }
}