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

            float cellSize = noiseSettings.scale;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float worldX = sampleCentre.x + x + noiseSettings.offset.x;
                    float worldY = sampleCentre.y - y + noiseSettings.offset.y;
                    
                    int cellX = Mathf.FloorToInt(worldX / cellSize);
                    int cellY = Mathf.FloorToInt(worldY / cellSize);

                    float minDist = float.MaxValue;

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = cellX + dx;
                            int ny = cellY + dy;

                            Vector2 featureOffset = Hash2(nx, ny, noiseSettings.seed) * cellSize;

                            float featureX = nx * cellSize + featureOffset.x;
                            float featureY = ny * cellSize + featureOffset.y;

                            float dxWorld = worldX - featureX;
                            float dyWorld = worldY - featureY;

                            float dist = Mathf.Sqrt(dxWorld * dxWorld + dyWorld * dyWorld);

                            if (dist < minDist)
                                minDist = dist;
                        }
                    }

                    map[x, y] = minDist / cellSize;
                }
            }

            return map;
        }
    }
}