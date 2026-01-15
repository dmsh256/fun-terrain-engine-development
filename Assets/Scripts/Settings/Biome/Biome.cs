using Generators.Noise;
using UnityEngine;

namespace Settings.Biome
{
    [CreateAssetMenu(menuName = "World/Biome")]
    public class Biome : ScriptableObject
    {
        public string biomeName;

        [Range(0,1)] public float minTemperature;
        [Range(0,1)] public float maxTemperature;

        [Range(0,1)] public float minMoisture;
        [Range(0,1)] public float maxMoisture;
        
        public NoiseSettings heightNoise;
        public AnimationCurve heightCurve;
        public float heightMultiplier;

        public Color debugColor;
    }
}