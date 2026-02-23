using System.Threading.Tasks;
using WorldGeneration;

namespace Generators.Noise.Generators
{
    using UnityEngine;

    public class VoronoiNoiseGenerator : INoiseGenerator
    {
        private static Vector2 Hash2(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;

                int h1 = h;
                int h2 = h * 374761393;

                float fx = (h1 & 0xffff) / 65535f;
                float fy = (h2 & 0xffff) / 65535f;

                return new Vector2(fx, fy);
            }
        }

        public static float[,] GenerateNoiseMap(int width, int height, NoiseSettings noiseSettings, Vector2 sampleCentre)
        {
            float[,] map = new float[width, height];
            float precomputedCellSize = 1f / noiseSettings.scale;
            
            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    float worldX = sampleCentre.x + x + noiseSettings.offset.x;
                    float worldZ = sampleCentre.y + y + noiseSettings.offset.y; 
                    // TODO experiment with this, the world must be generated from x->infinity, y->infinity
                    
                    int cellX = Mathf.FloorToInt(worldX * precomputedCellSize);
                    int cellY = Mathf.FloorToInt(worldZ * precomputedCellSize);

                    float minDist = float.MaxValue;

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = cellX + dx;
                            int ny = cellY + dy;

                            Vector2 featureOffset = Hash2(nx, ny, WorldContext.Seed) * noiseSettings.scale;

                            float featureX = nx * noiseSettings.scale + featureOffset.x;
                            float featureZ = ny * noiseSettings.scale + featureOffset.y;

                            float dxWorld = worldX - featureX;
                            float dyWorld = worldZ - featureZ;

                            float dist = Mathf.Sqrt(dxWorld * dxWorld + dyWorld * dyWorld);

                            if (dist < minDist)
                                minDist = dist;
                        }
                    }

                    map[x, y] = minDist / noiseSettings.scale;
                }
            });

            return map;
        }
    }
}