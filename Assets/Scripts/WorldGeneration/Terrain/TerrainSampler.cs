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
                float scale = terrainSpawnData.meshSettings.meshScale;

                float heightMapX = position.x / scale + 1;
                float heightMapZ = position.z / scale + 1;
                
                int x0 = Mathf.FloorToInt(heightMapX);
                int z0 = Mathf.FloorToInt(heightMapZ);
                int x1 = x0 + 1;
                int z1 = z0 + 1;

                float tx = heightMapX - x0;
                float tz = heightMapZ - z0;

                float h00 = terrainSpawnData.heightMap.GetRawHeight(x0, z0);
                float h10 = terrainSpawnData.heightMap.GetRawHeight(x1, z0);
                float h01 = terrainSpawnData.heightMap.GetRawHeight(x0, z1);
                float h11 = terrainSpawnData.heightMap.GetRawHeight(x1, z1);

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

            float hL = SampleHeightContinuous(terrainSpawnData, new Vector3(px - spacing, 0f, pz));
            float hR = SampleHeightContinuous(terrainSpawnData, new Vector3(px + spacing, 0f, pz));
            float hD = SampleHeightContinuous(terrainSpawnData, new Vector3(px, 0f, pz - spacing));
            float hU = SampleHeightContinuous(terrainSpawnData, new Vector3(px, 0f, pz + spacing));

            float dX = (hR - hL) * 0.5f;
            float dZ = (hU - hD) * 0.5f;

            Vector3 normal;
            normal.x = -dX;
            normal.y = 1f;
            normal.z = -dZ;

            return normal.normalized;
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