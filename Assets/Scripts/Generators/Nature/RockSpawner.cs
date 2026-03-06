using System.Collections.Generic;
using Generators.ObjectGenerator;
using Settings.Biome;
using Settings.Prefabs;
using UnityEngine;
using WorldGeneration.ObjectDistributionStrategies;
using WorldGeneration.Terrain;

namespace Generators.Nature
{
    public class RockSpawner : INatureObjectSpawner
    {
        private const float maxLeanAngle = 24f;
        private readonly List<Vector3> positions = new(2048);

        public void Spawn(ObjectSpawnContext objectSpawnContext, IObjectDistributionStrategy objectDistributionStrategy,
            int seed, System.Action<GameObject> emit, float spacing = 6f)
        {
            positions.Clear();
            objectDistributionStrategy.GeneratePositions(objectSpawnContext.terrainSpawnData, seed, spacing, positions);
            
            for (int i = 0; i < positions.Count; i++)
            {
               Vector3 localPosition = positions[i];
               
               float terrainHeight =
                    TerrainSampler.SampleHeightContinuous(objectSpawnContext.terrainSpawnData, localPosition);
               
               Vector3 worldPosition =
                   new Vector3(objectSpawnContext.terrainSpawnData.chunkCoordinates.x * objectSpawnContext.terrainSpawnData.meshSettings.meshWorldSize,
                       terrainHeight,
                       objectSpawnContext.terrainSpawnData.chunkCoordinates.y * objectSpawnContext.terrainSpawnData.meshSettings.meshWorldSize) 
                   + localPosition;
             
                if (!objectSpawnContext.biomeProvider.GetBiomeAtWorld(localPosition, objectSpawnContext.terrainSpawnData.meshSettings.meshScale, out BiomeData biome))
                    continue;

                if (!biome.hasRocks)
                    continue;

                if (biome.rocks == null)
                    continue;

                SpawnablePrefab spawnable = biome.rocks[Random.Range(0, biome.rocks.Length)];
                float densityNoise = Mathf.PerlinNoise(localPosition.x * 0.05f, localPosition.z * 0.05f);
                if (densityNoise > spawnable.density)
                    continue;
                
                Vector3 terrainNormal = TerrainSampler.SampleNormalContinuous(objectSpawnContext.terrainSpawnData, localPosition); 
                float slopeAngle = Vector3.Angle(Vector3.up, terrainNormal);
                if (slopeAngle > 45f)
                    continue;

                Vector3 placePosition = worldPosition;
                placePosition.y = terrainHeight + spawnable.yOffset;

                GameObject prefab = spawnable.prefab;

                float leanAngle = Mathf.Min(slopeAngle, maxLeanAngle);
                Vector3 leanAxis = Vector3.Cross(Vector3.up, terrainNormal);
                if (leanAxis.sqrMagnitude < 1e-6f)
                    leanAxis = Vector3.forward;
                else
                    leanAxis.Normalize();

                Quaternion leanRotation = Quaternion.AngleAxis(leanAngle, leanAxis);
                Quaternion randomYaw = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                Quaternion finalRotation = leanRotation * randomYaw;
                
                float scale = Random.Range(spawnable.scaleMin, spawnable.scaleMax);
                GameObject instance = objectSpawnContext.objectPoolManager.Spawn(prefab, placePosition, finalRotation);

                instance.transform.localScale = Vector3.one * scale;
                
                emit(instance);
            }
        }
    }
}
