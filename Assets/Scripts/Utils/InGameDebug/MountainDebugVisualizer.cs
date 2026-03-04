using Settings;
using WorldGeneration.WorldStructuralModifiers.LakeConnectionsModifier;
using WorldGeneration.WorldStructuralModifiers.MountainModifier;
using UnityEngine;

namespace Utils.InGameDebug
{
    public class MountainDebugVisualizer : MonoBehaviour
    {
        public WorldSettings worldSettings;
        public HeightMapSettings heightMapSettings;
        public MeshSettings meshSettings;

        [Range(16, 256)]
        public int resolution = 64;

        public bool regenerate;

        private MountainStructuralContext context;

        private void OnValidate()
        {
            if (regenerate)
            {
                regenerate = false;
                Generate();
            }
        }

        private void Generate()
        {
            MountainGenerator mountainGenerator =
                new(worldSettings, heightMapSettings, meshSettings);

            // We need canyon context because mountains avoid canyons
            LakeConnectionGenerator lakeGenerator =
                new(worldSettings, heightMapSettings, meshSettings);

            LakeStructuralModifierContext lakeContext =
                lakeGenerator.GenerateContext(resolution);

            context = mountainGenerator.GenerateContext(resolution, lakeContext);
        }

        private void OnDrawGizmos()
        {
            if (context == null)
                return;

            DrawWorldBounds();
            DrawClusters();
        }

        private void DrawWorldBounds()
        {
            float worldWidth = worldSettings.worldSizeInChunksX * meshSettings.meshWorldSize;

            Gizmos.color = Color.gray;
            Gizmos.DrawWireCube(
                Vector3.zero,
                new Vector3(worldWidth, 0, worldWidth));
        }

        private void DrawClusters()
        {
            foreach (MountainGenerator.PeakCluster cluster in context.clusters)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(
                    new Vector3(cluster.center.x, 0, cluster.center.y),
                    20f);

                foreach (MountainGenerator.Peak peak in cluster.peaks)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(
                        new Vector3(peak.position.x, 0, peak.position.y),
                        12f);

                    Gizmos.color = new Color(1f, 0.5f, 0f);
                    Gizmos.DrawLine(
                        new Vector3(cluster.center.x, 0, cluster.center.y),
                        new Vector3(peak.position.x, 0, peak.position.y));
                }
            }
        }
    }
}