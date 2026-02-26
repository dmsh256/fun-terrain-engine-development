using Settings;
using UnityEngine;

namespace Managers.World
{
    public class WorldManager
    {
        private readonly WorldSettings worldSettings;
        private readonly MeshSettings meshSettings;

        public WorldManager(WorldSettings worldSettings, MeshSettings meshSettings)
        {
            this.worldSettings = worldSettings;
            this.meshSettings = meshSettings;
        }
        
        public bool IsChunkCoordInsideWorld(Vector2 coord)
        {
            int halfX = worldSettings.worldSizeInChunksX / 2;
            int halfY = worldSettings.worldSizeInChunksY / 2;

            return coord.x >= -halfX && coord.x < halfX && coord.y >= -halfY && coord.y < halfY;
        }
        
        public Vector2Int GetChunkCoord(Vector2 position)
        {
            int x = Mathf.RoundToInt(position.x / meshSettings.meshWorldSize);
            int y = Mathf.RoundToInt(position.y / meshSettings.meshWorldSize);
            
            return new Vector2Int(x, y);
        }
    }
}