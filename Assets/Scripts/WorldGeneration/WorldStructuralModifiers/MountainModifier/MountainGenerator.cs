using System.Collections.Generic;
using Generators;
using Settings;
using UnityEngine;
using WorldGeneration.Terrain;
using WorldGeneration.WorldStructuralModifiers.LakeConnectionsModifier;

namespace WorldGeneration.WorldStructuralModifiers.MountainModifier
{
    public class MountainGenerator
    {
        private readonly WorldSettings worldSettings;
        private readonly HeightMapSettings heightMapSettings;
        private readonly MeshSettings meshSettings;

        private const int distanceFromCoast = 50;
        
        public MountainGenerator(WorldSettings worldSettings, HeightMapSettings heightMapSettings, MeshSettings meshSettings)
        {
            this.worldSettings = worldSettings;
            this.heightMapSettings = heightMapSettings;
            this.meshSettings = meshSettings;
        }

        public MountainStructuralContext GenerateContext(int resolution, LakeStructuralModifierContext lakeContext = null)
        {
            MountainStructuralContext mountainStructuralContext = new();

            TerrainContextMapLowResSampler terrainContextMapLowResSampler = new(worldSettings, heightMapSettings, meshSettings);
            TerrainContextMap terrainContextMap = terrainContextMapLowResSampler.GetTerrainContextMapLowRes(resolution);

            GenerateClusters(mountainStructuralContext, terrainContextMap, resolution, lakeContext);
            GenerateRidges(mountainStructuralContext);

            return mountainStructuralContext;
        }
        
        private void GenerateClusters(MountainStructuralContext mountainStructuralContext, TerrainContextMap terrainContextMap,
            int resolution, LakeStructuralModifierContext lakeContext = null)
        {
            float worldWidth = worldSettings.worldSizeInChunksX * meshSettings.meshWorldSize;
            float step = worldWidth / (resolution - 1);

            int clusterCount = worldSettings.mountainSettings.mountainClusterCount;

            int width = terrainContextMap.heightMap.values.GetLength(0);
            int height = terrainContextMap.heightMap.values.GetLength(1);

            List<Vector2> validLandPositions = new();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float rawHeight = terrainContextMap.heightMap.getRawHeight(x, y);
                    if (rawHeight <= worldSettings.waterLevel + distanceFromCoast)
                        continue;

                    Vector2 worldPos = new (
                        terrainContextMap.sampledFrom.x + x * step,
                        terrainContextMap.sampledFrom.y + y * step
                    );

                    if (OverlapsCanyon(worldPos, lakeContext))
                        continue;

                    validLandPositions.Add(worldPos);
                }
            }

            if (validLandPositions.Count == 0)
                return;

            for (int i = 0; i < clusterCount; i++)
            {
                Vector2 worldPos = validLandPositions[Random.Range(0, validLandPositions.Count)];
                PeakCluster cluster = CreateCluster(worldPos);
                mountainStructuralContext.clusters.Add(cluster);
            }
        }
        
        private PeakCluster CreateCluster(Vector2 center)
        {
            int peakCount = Random.Range(1, 6);
            float radius = worldSettings.mountainSettings.mountainMaxRadius;

            List<Peak> peaks = new();
            for (int i = 0; i < peakCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * radius;
                Vector2 position = center + offset;

                float height = Random.Range(
                    worldSettings.mountainSettings.mountainMinHeight,
                    worldSettings.mountainSettings.mountainMaxHeight
                );

                peaks.Add(new Peak(position, height));
            }

            return new PeakCluster(center, peaks);
        }
        
        private void GenerateRidges(MountainStructuralContext context)
        {
            foreach (PeakCluster cluster in context.clusters)
            {
                for (int i = 0; i < cluster.peaks.Count - 1; i++)
                {
                    cluster.ridges.Add(
                        new RidgeSpine(cluster.peaks[i].position, cluster.peaks[i + 1].position)
                    );
                }
            }
        }
        
        private bool OverlapsCanyon(Vector2 position, LakeStructuralModifierContext lakeContext)
        {
            if (lakeContext == null)
                return false;
            
            foreach (LakeConnectionGenerator.CanyonPath canyon in lakeContext.canyonPaths)
            {
                if (!canyon.bounds.Contains(new Vector3(position.x, 0, position.y)))
                    continue;

                float minDistance = GetDistanceToPolyline(position, canyon.points);

                if (minDistance < canyon.trenchWidth * 2f)
                    return true;
            }

            return false;
        }
        
        private float GetDistanceToPolyline(Vector2 point, List<Vector2> line)
        {
            float min = float.MaxValue;

            for (int i = 0; i < line.Count - 1; i++)
            {
                float dist = DistancePointToSegment(point, line[i], line[i + 1]);
                if (dist < min)
                    min = dist;
            }

            return min;
        }
        
        private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            
            return Vector2.Distance(p, a + ab * t);
        }
        
        public struct Peak
        {
            public Vector2 position;
            public readonly float height;

            public Peak(Vector2 position, float height)
            {
                this.position = position;
                this.height = height;
            }
        }
        
        public class PeakCluster
        {
            public Vector2 center;
            public readonly List<Peak> peaks;
            public readonly List<RidgeSpine> ridges = new();

            public PeakCluster(Vector2 center, List<Peak> peaks)
            {
                this.center = center;
                this.peaks = peaks;
            }
        }
        
        public struct RidgeSpine
        {
            public Vector2 start;
            public Vector2 end;

            public RidgeSpine(Vector2 start, Vector2 end)
            {
                this.start = start;
                this.end = end;
            }
        }
    }
}