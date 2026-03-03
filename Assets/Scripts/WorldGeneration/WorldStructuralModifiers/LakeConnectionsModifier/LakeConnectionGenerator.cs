using System.Collections.Generic;
using System.Linq;
using Generators;
using Generators.HeightMap;
using Settings;
using UnityEngine;
using WorldGeneration.Terrain;

namespace WorldGeneration.WorldStructuralModifiers.LakeConnectionsModifier
{
    public class LakeConnectionGenerator
    {
        private readonly float maxConnectionDistance;
        private readonly float wiggleFrequency;
        private readonly float wiggleStrengthModifier;
        private readonly int connectionPathSegments;
        private readonly int trenchWidth;
        private readonly float trenchWidthWiggleStrength;
        
        private readonly TerrainContextMapGenerator terrainContextGenerator;
        private readonly WorldSettings worldSettings;
        private readonly HeightMapSettings heightMapSettings;
        private readonly MeshSettings meshSettings;

        public LakeConnectionGenerator(WorldSettings worldSettings, HeightMapSettings heightMapSettings, MeshSettings meshSettings)
        {
            this.worldSettings = worldSettings;
            this.heightMapSettings = heightMapSettings;
            this.meshSettings = meshSettings;

            maxConnectionDistance = worldSettings.canyonSettings.maxConnectionDistance;
            wiggleFrequency = worldSettings.canyonSettings.wiggleFrequency;
            wiggleStrengthModifier = worldSettings.canyonSettings.wiggleStrengthModifier;
            connectionPathSegments = worldSettings.canyonSettings.connectionPathSegments;
            trenchWidth = worldSettings.canyonSettings.trenchWidth;
        }
        
        public LakeStructuralModifierContext GenerateWorldContext(int resolution)
        {
            float worldWidth = worldSettings.worldSizeInChunksX * meshSettings.meshWorldSize * worldSettings.worldStep;
            float globalWorldStep = worldWidth / (resolution - 1);
            
            TerrainContextMapLowResSampler terrainContextMapLowResSampler = new (worldSettings, heightMapSettings, meshSettings);
            TerrainContextMap terrainContextMap = terrainContextMapLowResSampler.GetTerrainContextMapLowRes(resolution);
            
            bool[,] waterMask = BuildWaterMask(terrainContextMap.heightMap, worldSettings.waterLevel);
            bool[,] oceanVisited = new bool[resolution, resolution];
            
            FloodFillOcean(waterMask, oceanVisited);
            List<LakeData> lakes = ExtractBasins(waterMask, oceanVisited, globalWorldStep, terrainContextMap.sampledFrom);
            List<LakeConnection> connections = BuildCanyonConnections(lakes);
            List<CanyonPath> canyonPaths = BuildCanyonPaths(lakes, connections, worldWidth);
            
            return new LakeStructuralModifierContext
            {
                canyonPaths = canyonPaths,
                lakes = lakes,
                lakeConnections = connections
            };
        }
        
        private List<CanyonPath> BuildCanyonPaths(List<LakeData> lakes, List<LakeConnection> connections, float worldWidth)
        {
            List<CanyonPath> paths = new();
            float wiggleStrength = worldWidth * wiggleStrengthModifier;

            foreach (LakeConnection connection in connections)
            {
                Vector2 pointA = lakes[connection.pointA].lakeCenterWorldCoordinates;
                Vector2 pointB = lakes[connection.pointB].lakeCenterWorldCoordinates;

                float trenchRandomWidth = RandomizeTrenchWidth(pointA, pointB);

                Vector2 direction = (pointB - pointA).normalized;
                Vector2 perpendicular = new (-direction.y, direction.x);

                List<Vector2> points = new();
                points.Add(pointA);
                
                float totalLength = Vector2.Distance(pointA, pointB);
                for (int i = 1; i < connectionPathSegments; i++)
                {
                    float t = i / (float)connectionPathSegments;
                    Vector2 point = Vector2.Lerp(pointA, pointB, t);

                    float distanceAlong = totalLength * t;
                    float noise = Mathf.PerlinNoise(distanceAlong * wiggleFrequency, 0f);
                    float offset = (noise - 0.5f) * 2f * wiggleStrength;

                    point += perpendicular * offset;
                    points.Add(point);
                }

                points.Add(pointB);
                Bounds bounds = ComputeBounds(points, trenchRandomWidth);
                CanyonPath canyonPath = new() { points = points, bounds = bounds, trenchWidth = trenchRandomWidth };
                paths.Add(canyonPath);
            }

            return paths;
        }

        private float RandomizeTrenchWidth(Vector2 pointA, Vector2 pointB)
        {
            float baseWidth = trenchWidth;
            float seed = pointA.x * 0.001f + pointB.y * 0.001f;
            float variationNoise = Mathf.PerlinNoise(seed, 0f);
            float variation = variationNoise - 0.5f;
            float trenchRandomWidth = baseWidth + variation * baseWidth;
            
            return trenchRandomWidth;
        }

        private List<LakeConnection> BuildCanyonConnections(List<LakeData> lakes)
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

                float distance = Vector2.Distance(current.lakeCenterWorldCoordinates, nearest.lake.lakeCenterWorldCoordinates);
                if (distance > maxConnectionDistance)
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
        
        private Bounds ComputeBounds(List<Vector2> points, float trenchRandomWidth)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            foreach (Vector2 point in points)
            {
                if (point.x < minX) minX = point.x;
                if (point.x > maxX) maxX = point.x;
                if (point.y < minZ) minZ = point.y;
                if (point.y > maxZ) maxZ = point.y;
            }

            minX -= trenchRandomWidth;
            maxX += trenchRandomWidth;
            minZ -= trenchRandomWidth;
            maxZ += trenchRandomWidth;

            Vector3 center = new ((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
            Vector3 size = new (maxX - minX, 0f, maxZ - minZ);

            return new Bounds(center, size);
        }
        
        public struct LakeData
        {
            public Vector2 lakeCenterWorldCoordinates;
            public float lakeArea; // TODO filter by area
        }
        
        public struct LakeConnection
        {
            public int pointA;
            public int pointB;
        }
        
        public struct CanyonPath
        {
            public List<Vector2> points;
            public Bounds bounds;
            
            public float trenchWidth;
        }
    }
}