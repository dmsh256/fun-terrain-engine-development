using UnityEngine;

namespace Settings.WorldStructuralModifiers
{
    [System.Serializable]
    public class CanyonSettings
    {
        [Range(500f, 5000f)]
        public float maxConnectionDistance = 2500f;

        [Range(0f, 0.01f)]
        public float wiggleFrequency = 0.001f;

        [Range(2, 20)]
        public int connectionPathSegments = 6;

        [Range(10, 500)]
        public int trenchWidth = 300;
    }
}