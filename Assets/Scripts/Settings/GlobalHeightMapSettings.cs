using System.Collections.Generic;
using Generators.Noise;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(menuName = "World/Global Height Settings")]
    public class GlobalHeightMapSettings : ScriptableObject
    {
        [Header("Noise Layers")]
        public List<NoiseLayer> layers = new();

        [Header("Height")]
        public float heightMultiplier = 1f;

        [Header("Height Curve")]
        public AnimationCurve heightCurve;
        public bool useHeightCurve = true;
        public bool useHeightMultiplier = true;
    }
}