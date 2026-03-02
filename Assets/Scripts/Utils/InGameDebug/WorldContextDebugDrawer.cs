using WorldGeneration.WorldStructuralModifiers.LakeConnectionsModifier;

namespace Utils.InGameDebug
{
    using UnityEngine;

    public class WorldContextDebugDrawer : MonoBehaviour
    {
        public static LakeStructuralModifierContext LakeStructuralModifierContext;

        private void OnDrawGizmos()
        {
            if (LakeStructuralModifierContext == null)
                return;

            DrawLakes();
            DrawConnections();
            DrawPaths();
        }

        private void DrawLakes()
        {
            Gizmos.color = Color.cyan;

            foreach (LakeConnectionGenerator.LakeData lakeData in LakeStructuralModifierContext.lakes)
            {
                Vector3 position = new (lakeData.lakeCenterWorldCoordinates.x, 50f, lakeData.lakeCenterWorldCoordinates.y);
                Gizmos.DrawSphere(position, 100f);
            }
        }

        private void DrawConnections()
        {
            Gizmos.color = Color.yellow;

            foreach (LakeConnectionGenerator.LakeConnection lakeConnection in LakeStructuralModifierContext.lakeConnections)
            {
                Vector2 a = LakeStructuralModifierContext.lakes[lakeConnection.pointA].lakeCenterWorldCoordinates;
                Vector2 b = LakeStructuralModifierContext.lakes[lakeConnection.pointB].lakeCenterWorldCoordinates;

                Gizmos.DrawLine(
                    new Vector3(a.x, 50f, a.y),
                    new Vector3(b.x, 50f, b.y)
                );
            }
        }

        private void DrawPaths()
        {
            Gizmos.color = Color.red;

            foreach (LakeConnectionGenerator.CanyonPath canyonPath in LakeStructuralModifierContext.canyonPaths)
            {
                for (int i = 0; i < canyonPath.points.Count - 1; i++)
                {
                    Vector2 a = canyonPath.points[i];
                    Vector2 b = canyonPath.points[i + 1];

                    Gizmos.DrawLine(
                        new Vector3(a.x, 60f, a.y),
                        new Vector3(b.x, 60f, b.y)
                    );
                }
            }
        }
    }
}