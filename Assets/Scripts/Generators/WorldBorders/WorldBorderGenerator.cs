using Settings;
using UnityEngine;

namespace Generators.WorldBorders
{
    public class WorldBorderGenerator
    {
        private readonly WorldSettings worldSettings;
        private readonly MeshSettings meshSettings;
        private readonly HeightMapSettings globalHeightMapSettings;

        public WorldBorderGenerator(WorldSettings worldSettings, MeshSettings meshSettings,
            HeightMapSettings globalHeightMapSettings)
        {
            this.worldSettings = worldSettings;
            this.meshSettings = meshSettings;
            this.globalHeightMapSettings = globalHeightMapSettings;
        }
        
        public void CreateWorldBorders()
        {
            Material borderMaterial = worldSettings.borderMaterial;
            
            float worldWidth  = worldSettings.worldSizeInChunksX * meshSettings.meshWorldSize;
            float worldHeight = worldSettings.worldSizeInChunksY * meshSettings.meshWorldSize;

            float halfWidth  = worldWidth * 0.5f;
            float halfHeight = worldHeight * 0.5f;

            float borderHeight = globalHeightMapSettings.heightMultiplier * 4f;
            const float thickness = 0f;

            CreateBorder(
                new Vector3(halfWidth, 0, 0),
                new Vector3(thickness, borderHeight, worldHeight),
                borderMaterial
            );

            CreateBorder(
                new Vector3(-halfWidth, 0, 0),
                new Vector3(thickness, borderHeight, worldHeight),
                borderMaterial
            );

            CreateBorder(
                new Vector3(0, 0,  halfHeight),
                new Vector3(worldWidth, borderHeight, thickness),
                borderMaterial
            );

            CreateBorder(
                new Vector3(0, 0, -halfHeight),
                new Vector3(worldWidth, borderHeight, thickness),
                borderMaterial
            );
        }

        private void CreateBorder(Vector3 position, Vector3 scale, Material material)
        {
            GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
            border.name = "WorldBorder";
            border.transform.position = position;
            border.transform.localScale = scale;
            border.GetComponent<MeshRenderer>().material = material;
        }
    }
}