using Generators.Terrain;
using UnityEngine;

namespace Utils.InGameDebug
{
    public class TerrainDebugDrawer : MonoBehaviour
    {
        public TerrainGenerator world; 

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            if (!world)
                return;

            foreach (TerrainChunk chunk in world.GetVisibleTerrainChunks())
            {
                //chunk.DebugDrawWeights(DrawLine);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

           /* foreach (TerrainChunk chunk in world.GetVisibleTerrainChunks())
                chunk.DebugDrawWeights(DrawLine);*/
        }

        private void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(start, end);
        }
    }
}