using System.Collections.Generic;
using UnityEngine;
using WorldGeneration.Biomes;

namespace WorldGeneration.ObjectDistributionStrategies
{
    public class JitteredGridDistribution : IObjectDistributionStrategy
    {
        public void GeneratePositions(TerrainSpawnData terrainSpawnData, int seed, float spacing, List<Vector3> positions)
        {
            int resolution = Mathf.RoundToInt(terrainSpawnData.meshSettings.meshWorldSize / spacing);

            System.Random rng = new(
                seed ^ (terrainSpawnData.chunkCoordinates.x * 73856093)
                     ^ (terrainSpawnData.chunkCoordinates.y * 19349663)
            );
            
            for (int gridZ = 0; gridZ < resolution; gridZ++)
            for (int gridX = 0; gridX < resolution; gridX++)
            {
                float jitterX = (float)rng.NextDouble() * spacing;
                float jitterZ = (float)rng.NextDouble() * spacing;

                Vector3 localOffset = new (
                    gridX * spacing + jitterX,
                    0f,
                    gridZ * spacing + jitterZ
                );

                positions.Add(localOffset);
            }
        }
    }
}