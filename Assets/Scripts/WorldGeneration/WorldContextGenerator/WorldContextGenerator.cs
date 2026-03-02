using System.Collections.Generic;
using System.Linq;
using Generators;
using Generators.HeightMap;
using Settings;
using UnityEngine;

namespace WorldGeneration.WorldContextGenerator
{
    public class WorldContextGenerator
    {
        private readonly TerrainContextMapGenerator terrainContextGenerator;
        private readonly WorldSettings worldSettings;
        private readonly HeightMapSettings heightMapSettings;

        public WorldContextGenerator(WorldSettings worldSettings, HeightMapSettings heightMapSettings)
        {
            this.worldSettings = worldSettings;
            this.heightMapSettings = heightMapSettings;
        }
        
        public WorldContext GenerateWorldContext(int resolution)
        {
            float worldWidth = worldSettings.worldSizeInChunksX * (242) * worldSettings.worldStep;
            float globalWorldStep = worldWidth / (resolution - 1);

            int minChunkIndex = worldSettings.worldSizeInChunksX / 2;
            int maxChunkIndex = worldSettings.worldSizeInChunksX / 2 - 1;

            int worldMinX = -minChunkIndex * 242;
            int worldMaxX = -(maxChunkIndex + 1) * 242;
            
            Vector2 worldBottomLeft = new (worldMinX, worldMaxX);

            float originalWorldStep = worldSettings.worldStep;
            worldSettings.worldStep = globalWorldStep;
            
            TerrainContextMapGenerator terrainContextGenerator = new (worldSettings);
            TerrainContextMap globalMap = terrainContextGenerator.GenerateTerrainContextMap(resolution,
                    resolution, heightMapSettings, worldBottomLeft, worldSettings.biomes);

            worldSettings.worldStep = originalWorldStep;

            bool[,] waterMask = BuildWaterMask(globalMap.heightMap, worldSettings.waterLevel);
            
            bool[,] oceanVisited = new bool[resolution, resolution];
            FloodFillOcean(waterMask, oceanVisited);
            List<LakeData> lakes = ExtractBasins(waterMask, oceanVisited, globalWorldStep, worldBottomLeft);
            
            Debug.Log(lakes.Count);
            
            float maxDistance = 2500; // tweak

            List<LakeConnection> connections =
                BuildCanyonConnections(lakes, maxDistance);
            
            List<CanyonPath> canyonPaths = BuildCanyonPaths(lakes, connections, worldWidth);
            
            Debug.Log(connections.Count);
            
            return new WorldContext
            {
                globalScaledHeightMap = globalMap.heightMap,
                globalScaledBiomeMap = globalMap.biomeDensityMap,
                resolution = resolution,
                worldStep = globalWorldStep,
                CanyonPaths = canyonPaths,
                lakes = lakes,
                connections = connections
            };
        }
        
        public struct LakeConnection
        {
            public int pointA;
            public int pointB;
        }
        
        public class CanyonPath
        {
            public List<Vector2> points;
        }
        
        private List<CanyonPath> BuildCanyonPaths(List<LakeData> lakes, List<LakeConnection> connections, float worldSize)
        {
            List<CanyonPath> paths = new();

            const float wiggleFrequency = 0.001f;
            float wiggleStrength  = worldSize * 0.05f;
            const int segments = 20;

            foreach (LakeConnection c in connections)
            {
                Vector2 pointA = lakes[c.pointA].lakeCenterWorldCoordinates;
                Vector2 pointB = lakes[c.pointB].lakeCenterWorldCoordinates;

                Vector2 direction = (pointB - pointA).normalized;
                Vector2 perpendicular = new (-direction.y, direction.x);

                List<Vector2> points = new();
                points.Add(pointA);
                for (int i = 1; i < segments; i++)
                {
                    float t = i / (float)segments;
                    Vector2 point = Vector2.Lerp(pointA, pointB, t);
                    float noise = Mathf.PerlinNoise(point.x * wiggleFrequency, point.y * wiggleFrequency);
                    float offset = (noise - 0.5f) * 2f * wiggleStrength;
                    point += perpendicular * offset;

                    points.Add(point);
                }

                points.Add(pointB);
                paths.Add(new CanyonPath { points = points });
            }

            return paths;
        }
        
        private List<LakeConnection> BuildCanyonConnections(List<LakeData> lakes, float maxDistance)
        {
            List<LakeConnection> connections = new();
            HashSet<(int,int)> used = new();
            Dictionary<int, int> degree = new();

            for (int i = 0; i < lakes.Count; i++)
                degree[i] = 0;

            for (int i = 0; i < lakes.Count; i++)
            {
                if (degree[i] >= 1)
                    continue;

                LakeData current = lakes[i];

                var nearest = lakes
                    .Select((lake, index) => new { lake, index })
                    .Where(x => x.index != i && degree[x.index] < 1)
                    .OrderBy(x => Vector2.Distance(current.lakeCenterWorldCoordinates, x.lake.lakeCenterWorldCoordinates))
                    .FirstOrDefault();

                if (nearest == null)
                    continue;

                float dist = Vector2.Distance(current.lakeCenterWorldCoordinates, nearest.lake.lakeCenterWorldCoordinates);

                if (dist > maxDistance)
                    continue;

                int a = Mathf.Min(i, nearest.index);
                int b = Mathf.Max(i, nearest.index);

                if (!used.Add((a, b)))
                    continue;

                connections.Add(new LakeConnection { pointA = a, pointB = b });

                degree[a]++;
                degree[b]++;
            }

            return connections;
        }
        
        private bool[,] BuildWaterMask(HeightMap heightMap, float waterLevel)
        {
            int size = heightMap.values.GetLength(0);
            bool[,] waterCells = new bool[size, size];

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                waterCells[x, y] = heightMap.getRawHeight(x, y) < waterLevel;
            
            return waterCells;
        }
        
        /**
         * not visited cells are supposed to be the lakes inside the world. The ones connected to the border - oceans
         */
        private void FloodFillOcean(bool[,] waterCells, bool[,] visited)
        {
            int size = waterCells.GetLength(0);
            Queue<Vector2Int> queue = new();
            for (int i = 0; i < size; i++)
            {
                TryEnqueue(i, 0);
                TryEnqueue(i, size - 1);
                TryEnqueue(0, i);
                TryEnqueue(size - 1, i);
            }

            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                foreach (Vector2Int direction in Directions4)
                {
                    int nx = cell.x + direction.x;
                    int ny = cell.y + direction.y;
                    if (nx >= 0 && nx < size && ny >= 0 && ny < size && waterCells[nx, ny] &&
                        !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }

            return;

            void TryEnqueue(int x, int y)
            {
                if (waterCells[x, y] && !visited[x, y])
                {
                    visited[x, y] = true;
                    queue.Enqueue(new Vector2Int(x, y));
                }
            }
        }
        
        private static readonly Vector2Int[] Directions4 =
        {
            new(1,0), new(-1,0), new(0,1), new(0,-1)
        };
        
        private List<LakeData> ExtractBasins(bool[,] waterMask, bool[,] oceanVisited, float worldStep, Vector2 worldBottomLeft)
        {
            int size = waterMask.GetLength(0);
            bool[,] visited = new bool[size, size];

            List<LakeData> lakes = new();
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (!waterMask[x, y] || oceanVisited[x, y] || visited[x, y])
                        continue;

                    lakes.Add(FloodFillLake(x, y));
                }
            }

            return lakes;

            LakeData FloodFillLake(int startX, int startY)
            {
                Queue<Vector2Int> queue = new();
                queue.Enqueue(new Vector2Int(startX, startY));
                visited[startX, startY] = true;

                int count = 0;
                float sumX = 0;
                float sumY = 0;
                while (queue.Count > 0)
                {
                    Vector2Int cell = queue.Dequeue();

                    count++;
                    sumX += cell.x;
                    sumY += cell.y;
                    foreach (Vector2Int directions in Directions4)
                    {
                        int nx = cell.x + directions.x;
                        int ny = cell.y + directions.y;
                        if (nx >= 0 && nx < size && ny >= 0 && ny < size && waterMask[nx, ny] && !oceanVisited[nx, ny] &&
                            !visited[nx, ny])
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }

                float centerX = sumX / count;
                float centerY = sumY / count;
                Vector2 lakeCenterWorldCoordinates = new(worldBottomLeft.x + centerX * worldStep, worldBottomLeft.y + centerY * worldStep);
                
                return new LakeData
                {
                    lakeCenterWorldCoordinates = lakeCenterWorldCoordinates,
                    lakeArea = count * worldStep * worldStep
                };
            }
        }
        
        public class LakeData
        {
            public Vector2 lakeCenterWorldCoordinates;
            public float lakeArea;
        }
    }
}