using Generators.HeightMap;
using Settings;
using UnityEngine;

namespace WorldGeneration.Biomes
{
    public class TerrainSpawnData
    {
        public readonly Vector2Int chunkCoordinates;
        public readonly MeshSettings meshSettings;
        public readonly HeightMap heightMap;
        public readonly LayerMask terrainLayerMask;

        public TerrainSpawnData(Vector2Int chunkCoordinates, MeshSettings meshSettings, HeightMap heightMap, LayerMask terrainLayerMask)
        {
            this.chunkCoordinates = chunkCoordinates;
            this.meshSettings = meshSettings;
            this.heightMap = heightMap;
            this.terrainLayerMask = terrainLayerMask;
        }
    }
}