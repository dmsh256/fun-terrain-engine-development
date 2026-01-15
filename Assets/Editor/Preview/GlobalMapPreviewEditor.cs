using Utils.Preview;
using UnityEditor;
using UnityEngine;

namespace Editor.Preview
{
    [CustomEditor(typeof(GlobalMapPreview))]
    public class GlobalMapPreviewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var globalMapPreview = (GlobalMapPreview)target;

            if (DrawDefaultInspector())
            {
                if (globalMapPreview.autoUpdate)
                {
                    globalMapPreview.DrawMultipleChunksInEditor();
                }
            }

            if (GUILayout.Button("Generate"))
            {
                globalMapPreview.DrawMultipleChunksInEditor();
            }
        }
    }
}