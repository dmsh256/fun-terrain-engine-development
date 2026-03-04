using Generators;
using Generators.HeightMap;
using Settings;
using UnityEngine;

namespace WorldGeneration.Terrain
{
    public class TerrainContextMapLowResSampler
    {
        private readonly WorldSettings worldSettings;
        private readonly HeightMapSettings heightMapSettings;
        private readonly MeshSettings meshSettings;
        
        public TerrainContextMapLowResSampler(WorldSettings worldSettings, HeightMapSettings heightMapSettings, MeshSettings meshSettings)
        {
            this.worldSettings = worldSettings;
            this.heightMapSettings = heightMapSettings;
            this.meshSettings = meshSettings;
        }

        /**
         * supports only square worlds
         */
        public TerrainContextMap GetTerrainContextMapLowRes(int resolution)
        {
            float worldWidth = worldSettings.worldSizeInChunksX * meshSettings.meshWorldSize;
            float samplingStep = worldWidth / (resolution - 1);

            int minChunkIndex = worldSettings.worldSizeInChunksX / 2;
            int maxChunkIndex = worldSettings.worldSizeInChunksX / 2 - 1;

            int worldMinX = -minChunkIndex * meshSettings.meshWorldSize;
            int worldMaxX = -(maxChunkIndex + 1) * meshSettings.meshWorldSize;
            
            Vector2 worldBottomLeft = new (worldMinX, worldMaxX);
            
            TerrainContextMapGenerator terrainContextGenerator = new (worldSettings);
            TerrainContextMap globalTerrainContextMap = terrainContextGenerator.GenerateTerrainContextMap(resolution,
                resolution, heightMapSettings, worldBottomLeft, worldSettings.biomes, samplingStep);
            
            return globalTerrainContextMap;
        }
    }
}