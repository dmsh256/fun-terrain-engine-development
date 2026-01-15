using Generators.Noise;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(menuName = "World/Global Height Settings")]
    public class GlobalHeightMapSettings : ScriptableObject
    {
        [Header("Base Shape")]
        public float heightMultiplier;

        public NoiseSettings maskSettings;
        public NoiseSettings noiseSettings;
        
        [Header("Height Curve")]
        public AnimationCurve heightCurve;
    }
}