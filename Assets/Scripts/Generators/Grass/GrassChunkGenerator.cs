using Settings.Biome;
using UnityEngine;
using WorldGeneration.Biomes;
using WorldGeneration.ObjectDistributionStrategies;
using WorldGeneration.Terrain;

namespace Generators.Grass
{
    public static class GrassChunkGenerator
    {
        private const float maxGrassLeanAngle = 12f;

        public static GrassChunk Generate(TerrainSpawnData terrainSpawnData, IBiomeProvider biomeProvider,
            IObjectDistributionStrategy objectDistributionStrategy, int seed, float spacing = 1f,
            Mesh fallbackMesh = null, Material fallbackMaterial = null)
        {
            GrassChunk chunk = new();
            int chunkSeed = seed ^ (terrainSpawnData.chunkCoordinates.x * 73856093) ^ (terrainSpawnData.chunkCoordinates.y * 19349663);
            System.Random rng = new(chunkSeed);

            foreach (Vector3 localPosition in
                     objectDistributionStrategy.GeneratePositions(terrainSpawnData, seed, spacing))
            {
                float terrainHeight =
                    TerrainSampler.SampleHeightContinuous(terrainSpawnData, localPosition);

                Vector3 worldPosition =
                    new Vector3(terrainSpawnData.chunkCoordinates.x * terrainSpawnData.meshSettings.meshWorldSize,
                        terrainHeight,
                        terrainSpawnData.chunkCoordinates.y * terrainSpawnData.meshSettings.meshWorldSize) 
                    + localPosition;
                
                if (!biomeProvider.GetBiomeAtWorld(localPosition, terrainSpawnData.meshSettings.meshScale, out BiomeData biome))
                    continue;

                if (!biome.hasGrass)
                    continue;

                float densityNoise = Mathf.PerlinNoise(localPosition.x * 0.05f, localPosition.z * 0.05f);
                if (densityNoise < biome.grassDensity)
                    continue;

                if (!TryPickVariant(biome.grassVariants, rng, out BiomeGrassVariant grassVariant))
                {
                    if (!fallbackMesh || !fallbackMaterial)
                        continue;

                    grassVariant = new BiomeGrassVariant
                    {
                        mesh = fallbackMesh,
                        material = fallbackMaterial,
                        weight = 1f,
                        scale = 1f
                    };
                }

                Vector3 terrainNormal = TerrainSampler.SampleNormalContinuous(terrainSpawnData, localPosition); 
                float slopeAngle = Vector3.Angle(Vector3.up, terrainNormal);
               
                if (slopeAngle > 35f)
                    continue;

                float leanAngle = Mathf.Min(slopeAngle, maxGrassLeanAngle);

                Vector3 leanAxis = Vector3.Cross(Vector3.up, terrainNormal);
                if (leanAxis.sqrMagnitude < 1e-6f)
                    leanAxis = Vector3.forward;
                else
                    leanAxis.Normalize();

                Quaternion leanRotation = Quaternion.AngleAxis(leanAngle, leanAxis);
                Quaternion randomYaw =
                    Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);

                Quaternion finalRotation = leanRotation * randomYaw;

                chunk.AddInstance(grassVariant.mesh, grassVariant.material, new GrassInstance
                {
                    position = new Vector3(
                        worldPosition.x, 
                        terrainHeight + grassVariant.yOffset,
                        worldPosition.z
                    ),
                    rotation = finalRotation,
                    scale = Mathf.Lerp(0.8f, 1.2f, densityNoise) * Mathf.Max(0.01f, grassVariant.scale),
                });
            }

            return chunk;
        }

        private static bool TryPickVariant(BiomeGrassVariant[] variants, System.Random rng, out BiomeGrassVariant selected)
        {
            selected = default;
            if (variants == null || variants.Length == 0)
                return false;

            float totalWeight = 0f;
            foreach (BiomeGrassVariant variant in variants)
            {
                if (!variant.mesh || !variant.material)
                    continue;

                float weight = Mathf.Max(0f, variant.weight);
                if (weight <= 0f)
                    continue;

                totalWeight += weight;
            }

            if (totalWeight <= 0f)
                return false;

            float roll = (float)rng.NextDouble() * totalWeight;
            float cumulative = 0f;
            foreach (BiomeGrassVariant variant in variants)
            {
                if (!variant.mesh || !variant.material)
                    continue;

                float weight = Mathf.Max(0f, variant.weight);
                if (weight <= 0f)
                    continue;

                cumulative += weight;
                if (roll <= cumulative)
                {
                    selected = variant;
                    return true;
                }
            }

            return false;
        }
    }
}
