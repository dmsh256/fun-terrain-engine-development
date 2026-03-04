using System.Collections.Generic;
using Generators.ObjectGenerator;
using Settings.Biome;
using Settings.Prefabs;
using UnityEngine;
using WorldGeneration.ObjectDistributionStrategies;
using WorldGeneration.Terrain;

namespace Generators.Nature
{
    public class TreeSpawner : INatureObjectSpawner
    {
        private const float maxLeanAngle = 8f;

        public IEnumerable<GameObject> Spawn(ObjectSpawnContext objectSpawnContext, IObjectDistributionStrategy objectDistributionStrategy,
            int seed, float spacing = 6f)
        {
            foreach (Vector3 localPosition in
                     objectDistributionStrategy.GeneratePositions(objectSpawnContext.terrainSpawnData, seed, spacing))
            {
                float terrainHeight =
                    TerrainSampler.SampleHeightContinuous(objectSpawnContext.terrainSpawnData, localPosition);
                
                Vector3 worldPosition =
                    new Vector3(objectSpawnContext.terrainSpawnData.chunkCoordinates.x * objectSpawnContext.terrainSpawnData.meshSettings.meshWorldSize,
                        terrainHeight,
                        objectSpawnContext.terrainSpawnData.chunkCoordinates.y * objectSpawnContext.terrainSpawnData.meshSettings.meshWorldSize) 
                    + localPosition;
                
                if (!objectSpawnContext.biomeProvider.GetBiomeAtWorld(localPosition, objectSpawnContext.terrainSpawnData.meshSettings.meshScale, out BiomeData biome))
                    continue;

                if (!biome.hasTrees)
                    continue;

                if (biome.trees == null)
                    continue;

                SpawnablePrefab spawnable = biome.trees[Random.Range(0, biome.trees.Length)];
                float densityNoise =
                    Mathf.PerlinNoise(localPosition.x * 0.05f, localPosition.z * 0.05f);
                if (densityNoise > spawnable.density)
                    continue;

                Vector3 terrainNormal = TerrainSampler.SampleNormalContinuous(objectSpawnContext.terrainSpawnData, localPosition); 
                
                float slopeAngle = Vector3.Angle(Vector3.up, terrainNormal);
                if (slopeAngle > 25f)
                    continue;

                Vector3 placePosition = new()
                {
                    x = worldPosition.x,
                    y = terrainHeight + spawnable.yOffset,
                    z = worldPosition.z
                };

                GameObject prefab = spawnable.prefab;
                prefab.name = "Tree " + " at " + worldPosition.x + " " + worldPosition.z + " " + worldPosition.y + " with normal: " + terrainNormal + ", slope " + slopeAngle;

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
                GameObject instance = Object.Instantiate(prefab, placePosition, finalRotation, objectSpawnContext.parent);

                instance.transform.localScale = Vector3.one * scale;
                
                yield return instance;
            }
        }
    }
}
