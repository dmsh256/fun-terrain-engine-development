using System;
using UnityEngine;
using WorldGeneration.Biomes;

namespace WorldGeneration.Terrain
{
    public static class TerrainSampler
    {
        public static float SampleHeightContinuous(TerrainSpawnData terrainSpawnData, Vector3 position)
        {
            try
            {
                int x0 = Mathf.FloorToInt(position.x);
                int z0 = Mathf.FloorToInt(position.z);
                int x1 = x0 + 1;
                int z1 = z0 + 1;

                float tx = position.x - x0;
                float tz = position.z - z0;

                float h00 = terrainSpawnData.heightMap.getRawHeight(x0, z0);
                float h10 = terrainSpawnData.heightMap.getRawHeight(x1, z0);
                float h01 = terrainSpawnData.heightMap.getRawHeight(x0, z1);
                float h11 = terrainSpawnData.heightMap.getRawHeight(x1, z1);

                float h0 = Mathf.Lerp(h00, h10, tx);
                float h1 = Mathf.Lerp(h01, h11, tx);

                return Mathf.Lerp(h0, h1, tz);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.Message + " world position: " + position);

                return 0f;
            }
        }
        
        public static Vector3 SampleNormalContinuous(TerrainSpawnData terrainSpawnData, Vector3 position)
        {
            const float spacing = 1f;

            float px = Mathf.Clamp(position.x, spacing, terrainSpawnData.meshSettings.meshWorldSize - spacing);
            float pz = Mathf.Clamp(position.z, spacing, terrainSpawnData.meshSettings.meshWorldSize - spacing);

            Vector3 p = new (px, 0f, pz);
            float hL = SampleHeightContinuous(terrainSpawnData, p + new Vector3(-spacing, 0f, 0f));
            float hR = SampleHeightContinuous(terrainSpawnData, p + new Vector3( spacing, 0f, 0f));
            float hD = SampleHeightContinuous(terrainSpawnData, p + new Vector3( 0f, 0f,-spacing));
            float hU = SampleHeightContinuous(terrainSpawnData, p + new Vector3( 0f, 0f, spacing));

            float dX = (hR - hL) / (2f * spacing);
            float dZ = (hU - hD) / (2f * spacing);

            return new Vector3(-dX, 1f, -dZ).normalized;
        }
        
        public static class TerrainRaycastSampler
        {
            public static bool Sample(Vector3 worldPosition, TerrainSpawnData terrainSpawnData, out Vector3 hitPos, out Vector3 hitNormal, float rayHeight = 10f, float rayLength = 3f)
            {
                Vector3 rayStart = new (worldPosition.x, rayHeight + 2f, worldPosition.z);
                
                Ray ray = new (rayStart, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, rayLength, terrainSpawnData.terrainLayerMask,
                        QueryTriggerInteraction.Ignore))
                {
                    hitPos    = hit.point;
                    hitNormal = hit.normal;
                    
                    return true;
                }

                hitPos = default;
                hitNormal = Vector3.up;
                
                return false;
            }
        }
    }
}