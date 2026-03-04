using UnityEngine;

namespace Settings.WorldStructuralModifiers
{
    [System.Serializable]
    public class CanyonSettings
    {
        public bool canyonsBetweenLakesEnabled = true;
        
        [Range(500f, 5000f)]
        public float maxConnectionDistance = 2500f;

        [Range(0f, 0.01f)]
        public float wiggleFrequency = 0.001f;

        [Range(0.01f, 0.1f)]
        public float wiggleStrengthModifier = 0.005f;
        
        [Range(2, 20)]
        public int connectionPathSegments = 6;

        [Range(10, 500)]
        public int trenchWidth = 300;
    }
}