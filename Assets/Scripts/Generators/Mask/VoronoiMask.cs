using Generators.Noise;
using UnityEngine;

namespace Generators.Mask
{
    public class VoronoiMask : IMask
    {
        private readonly float cellSize;
        private readonly int seed;

        private static Vector2 Hash2(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;

                float fx = (h & 0xffff) / 65535f;
                float fy = ((h >> 16) & 0xffff) / 65535f;
                
                return new Vector2(fx, fy);
            }
        }
        
        public VoronoiMask(NoiseSettings settings)
        {
            cellSize = settings.scale;
            seed = settings.seed;
        }

        public float Sample(float worldX, float worldZ)
        {
            int cellX = Mathf.FloorToInt(worldX / cellSize);
            int cellZ = Mathf.FloorToInt(worldZ / cellSize);

            float minDist = float.MaxValue;

            for (int dz = -1; dz <= 1; dz++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = cellX + dx;
                    int nz = cellZ + dz;

                    Vector2 p = Hash2(nx, nz, seed) * cellSize;
                    float fx = nx * cellSize + p.x;
                    float fz = nz * cellSize + p.y;

                    float dist = Vector2.Distance(
                        new Vector2(worldX, worldZ),
                        new Vector2(fx, fz)
                    );

                    minDist = Mathf.Min(minDist, dist);
                }

            float maxDist = cellSize * 1.41421356f;
            
            return Mathf.Clamp01(minDist / maxDist);
        }
    }
}