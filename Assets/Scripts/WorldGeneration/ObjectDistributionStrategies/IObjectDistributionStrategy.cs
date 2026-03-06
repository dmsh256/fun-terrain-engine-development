using System.Collections.Generic;
using UnityEngine;
using WorldGeneration.Biomes;

namespace WorldGeneration.ObjectDistributionStrategies
{
    public interface IObjectDistributionStrategy
    {
        void GeneratePositions(TerrainSpawnData terrainSpawnData, int seed, float spacing, List<Vector3> buffer);
    }
}