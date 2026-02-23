namespace Utils.InGameDebug
{
    using UnityEngine;

    public class DebugWorldPosWindow : MonoBehaviour
    {
        public Transform target;

        private Rect windowRect = new (10, 10, 260, 120);

        void OnGUI()
        {
            windowRect = GUI.Window(10, windowRect, DrawWindow, "Debug");
        }

        void DrawWindow(int id)
        {
            if (!target)
            {
                GUILayout.Label("No target assigned");
            }
            else
            {
                Vector3 p = target.position;

                GUILayout.Label("World Pos:");
                GUILayout.Label($"X: {p.x:F2}");
                GUILayout.Label($"Y: {p.y:F2}");
                GUILayout.Label($"Z: {p.z:F2}");
            }

            GUI.DragWindow();
        }
    }
}