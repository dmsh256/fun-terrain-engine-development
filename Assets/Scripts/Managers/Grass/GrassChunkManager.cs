using System.Collections.Generic;
using Generators.Grass;
using Settings;
using UnityEngine;
using WorldGeneration;
using WorldGeneration.Biomes;
using WorldGeneration.ObjectDistributionStrategies;

namespace Managers.Grass
{
    public class GrassChunkManager
    {
        private readonly Dictionary<Vector2, GrassChunk> grassChunks = new();
        private readonly List<GrassChunk> visibleGrassChunks = new();

        private readonly GrassIndirectRenderer grassRenderer;
        private readonly MeshSettings meshSettings;
        private readonly Mesh fallbackMesh;
        private readonly Material fallbackMaterial;

        public GrassChunkManager(GrassIndirectRenderer grassRenderer, MeshSettings meshSettings, Mesh fallbackMesh, Material fallbackMaterial)
        {
            this.grassRenderer = grassRenderer;
            this.meshSettings = meshSettings;
            this.fallbackMesh = fallbackMesh;
            this.fallbackMaterial = fallbackMaterial;
        }

        public void Spawn(TerrainChunk terrainChunk, IBiomeProvider biomeProvider)
        {
            if (grassChunks.ContainsKey(terrainChunk.coordinates))
                return;

            TerrainSpawnData terrainSpawnData = new(
                new Vector2Int(terrainChunk.coordinates.x, terrainChunk.coordinates.y),
                meshSettings,
                terrainChunk.terrainContextMap.heightMap,
                terrainChunk.terrainLayerMask);

            GrassChunk grassChunk = GrassChunkGenerator.Generate(
                terrainSpawnData,
                biomeProvider,
                new JitteredGridDistribution(), // TODO inject from somewhere
                WorldContextSettings.Seed,
                fallbackMesh: fallbackMesh,
                fallbackMaterial: fallbackMaterial);

            grassChunks.Add(terrainChunk.coordinates, grassChunk);

            if (terrainChunk.IsVisible())
                SetVisibility(terrainChunk.coordinates, true);
        }

        public void Remove(Vector2 coord)
        {
            if (!grassChunks.TryGetValue(coord, out GrassChunk grassChunk))
                return;

            grassChunk.Release();
            visibleGrassChunks.Remove(grassChunk);
            grassRenderer?.SetVisibleChunks(visibleGrassChunks);
            grassChunks.Remove(coord);
        }

        public void SetVisibility(Vector2 coord, bool visible)
        {
            if (!grassChunks.TryGetValue(coord, out GrassChunk grassChunk))
                return;

            if (visible)
            {
                if (!visibleGrassChunks.Contains(grassChunk))
                    visibleGrassChunks.Add(grassChunk);

                grassRenderer?.SetVisibleChunks(visibleGrassChunks);
                grassChunk.BuildBuffers();
            }
            else
            {
                visibleGrassChunks.Remove(grassChunk);
                grassRenderer?.SetVisibleChunks(visibleGrassChunks);
            }
        }
        
        public void Clear()
        {
            foreach (GrassChunk grassChunk in grassChunks.Values)
                grassChunk.Release();

            grassChunks.Clear();
            visibleGrassChunks.Clear();
            grassRenderer?.SetVisibleChunks(visibleGrassChunks);
        }
    }
}