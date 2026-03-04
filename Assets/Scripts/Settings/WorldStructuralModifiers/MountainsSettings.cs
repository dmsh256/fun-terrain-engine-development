using UnityEngine;

namespace Settings.WorldStructuralModifiers
{
    [System.Serializable]
    public class MountainsSettings
    {
        public bool mountainsEnabled = true;
        
        [Range(0, 100)]
        public int mountainClusterCount = 20;
        
        public float mountainMaxRadius;
        
        public float mountainMaxHeight;
        public float mountainMinHeight;
        
        public float mountainPeakRadius;
        public float mountainRidgeWidth;
        
        [Range(100, 1000)]
        public float amplificationStrength = 500f;
    }
}