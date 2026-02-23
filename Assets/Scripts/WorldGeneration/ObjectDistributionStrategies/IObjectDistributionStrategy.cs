using System.Collections.Generic;
using UnityEngine;
using WorldGeneration.Biomes;

namespace WorldGeneration.ObjectDistributionStrategies
{
    public interface IObjectDistributionStrategy
    {
        IEnumerable<Vector3> GeneratePositions(TerrainSpawnData terrainSpawnData, int seed, float spacing);
    }
}