using System.Collections.Generic;
using UnityEngine;
using WorldGeneration.Biomes;

namespace WorldGeneration.ObjectDistributionStrategies
{
    public class PoissonDiskDistribution : IObjectDistributionStrategy
    {
        private const int k = 30;

        public void GeneratePositions(TerrainSpawnData terrainSpawnData, int seed, float minDistance, List<Vector3> positions)
        {
            float chunkSize = terrainSpawnData.meshSettings.meshWorldSize;
            Vector2 chunkOrigin = new(0, 0);

            System.Random rng = new(
                seed ^ (terrainSpawnData.chunkCoordinates.x * 73856093)
                     ^ (terrainSpawnData.chunkCoordinates.y * 19349663)
            );

            float cellSize = minDistance / Mathf.Sqrt(2);
            int gridWidth  = Mathf.CeilToInt(chunkSize / cellSize);
            int gridHeight = Mathf.CeilToInt(chunkSize / cellSize);

            Vector2?[,] grid = new Vector2?[gridWidth, gridHeight];
            List<Vector2> active = new();
            List<Vector2> points = new();

            Vector2 first = new(
                (float)rng.NextDouble() * chunkSize,
                (float)rng.NextDouble() * chunkSize
            );

            points.Add(first);
            active.Add(first);

            grid[(int)(first.x / cellSize), (int)(first.y / cellSize)] = first;

            while (active.Count > 0)
            {
                int index = rng.Next(active.Count);
                Vector2 center = active[index];
                bool found = false;

                for (int i = 0; i < k; i++)
                {
                    float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                    float radius = minDistance * (1f + (float)rng.NextDouble());

                    Vector2 candidate = center + new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) * radius;

                    if (candidate.x < 0 || candidate.y < 0 ||
                        candidate.x >= chunkSize || candidate.y >= chunkSize)
                        continue;

                    int gx = (int)(candidate.x / cellSize);
                    int gy = (int)(candidate.y / cellSize);

                    bool valid = true;

                    for (int y = Mathf.Max(gy - 2, 0); y <= Mathf.Min(gy + 2, gridHeight - 1); y++)
                    for (int x = Mathf.Max(gx - 2, 0); x <= Mathf.Min(gx + 2, gridWidth - 1); x++)
                    {
                        if (grid[x, y].HasValue &&
                            Vector2.Distance(grid[x, y].Value, candidate) < minDistance)
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (!valid)
                        continue;

                    points.Add(candidate);
                    active.Add(candidate);
                    grid[gx, gy] = candidate;
                    found = true;
                    break;
                }

                if (!found)
                    active.RemoveAt(index);
            }
            
            foreach (Vector2 p in points)
            {
                positions.Add(new Vector3(
                    chunkOrigin.x + p.x,
                    0f,
                    chunkOrigin.y + p.y)
                );
            }
        }
    }
}