using WorldGeneration.WorldContextGenerator;

namespace Utils.InGameDebug
{
    using UnityEngine;

    public class WorldContextDebugDrawer : MonoBehaviour
    {
        public static WorldContext worldContext;

        private void OnDrawGizmos()
        {
            if (worldContext == null)
                return;

            DrawLakes();
            DrawConnections();
            DrawPaths();
        }

        private void DrawLakes()
        {
            Gizmos.color = Color.cyan;

            foreach (WorldContextGenerator.LakeData lake in worldContext.lakes)
            {
                Vector3 pos = new (lake.lakeCenterWorldCoordinates.x, 50f, lake.lakeCenterWorldCoordinates.y);
                Gizmos.DrawSphere(pos, 100f);
            }
        }

        private void DrawConnections()
        {
            Gizmos.color = Color.yellow;

            foreach (WorldContextGenerator.LakeConnection c in worldContext.connections)
            {
                Vector2 a = worldContext.lakes[c.pointA].lakeCenterWorldCoordinates;
                Vector2 b = worldContext.lakes[c.pointB].lakeCenterWorldCoordinates;

                Gizmos.DrawLine(
                    new Vector3(a.x, 50f, a.y),
                    new Vector3(b.x, 50f, b.y)
                );
            }
        }

        private void DrawPaths()
        {
            Gizmos.color = Color.red;

            foreach (WorldContextGenerator.CanyonPath path in worldContext.CanyonPaths)
            {
                for (int i = 0; i < path.points.Count - 1; i++)
                {
                    Vector2 a = path.points[i];
                    Vector2 b = path.points[i + 1];

                    Gizmos.DrawLine(
                        new Vector3(a.x, 60f, a.y),
                        new Vector3(b.x, 60f, b.y)
                    );
                }
            }
        }
    }
}