using UnityEngine;

namespace Settings.Prefabs
{
    [System.Serializable]
    public class SpawnablePrefab
    {
        public GameObject prefab;

        [Range(0f, 1f)]
        public float density = 0.5f;

        [Range(-5f, 5f)]
        public float yOffset = -0.5f;

        [Range(0.1f, 35f)]
        public float scaleMin = 1f;
        
        [Range(0.1f, 35f)]
        public float scaleMax = 1f;
    }
}