using UnityEditor;
using UnityEngine;
using Utils.Preview;

namespace Editor.Preview
{
    [CustomEditor(typeof(MapPreview))]
    public class MapPreviewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var mapPreview = (MapPreview)target;

            if (DrawDefaultInspector())
            {
                if (mapPreview.drawMultipleChunks)
                {
                    if (mapPreview.autoUpdate)
                    {
                        mapPreview.DrawMultipleChunksInEditor();
                    }
                }
                else
                {
                    if (mapPreview.autoUpdate)
                    {
                        mapPreview.DrawSingleChunkInEditor();
                    }
                }
            }

            if (GUILayout.Button("Generate"))
            {
                if (mapPreview.drawMultipleChunks)
                {
                    mapPreview.DrawMultipleChunksInEditor();
                }
                else
                {
                    mapPreview.DrawSingleChunkInEditor();
                }
            }
        }
    }
}