using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generators.Grass
{
    public class GrassIndirectRenderer : MonoBehaviour
    {
        [Header("Viewer")]
        [SerializeField] private Transform viewer;

        [Header("Shader")]
        [SerializeField] private Shader indirectShader;

        private Vector2 viewerXZ;
        private Vector2 lastRebuildViewerXZ;
        
        private IReadOnlyList<GrassChunk> visibleChunks = Array.Empty<GrassChunk>();
        
        public void SetVisibleChunks(IReadOnlyList<GrassChunk> chunks)
        {
            visibleChunks = chunks ?? Array.Empty<GrassChunk>();
        }

        private void Awake()
        {
            if (viewer) return;
            
            Debug.LogError("GrassIndirectRenderer: Viewer not assigned", this);
            enabled = false;
        }

        private void LateUpdate()
        {
            for (int i = 0; i < visibleChunks.Count; i++)
            {
                visibleChunks[i].Draw();
            }
        }
    }
}
