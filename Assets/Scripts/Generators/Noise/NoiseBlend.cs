using UnityEngine;

namespace Generators.Noise
{
    public class NoiseBlend
    {
        public static float Add(float a, float b) => a + b;
        public static float Multiply(float a, float b) => a * b;
        public static float Max(float a, float b) => Mathf.Max(a, b);
        public static float Min(float a, float b) => Mathf.Min(a, b);

        public static float Lerp(float a, float b, float t)
            => Mathf.Lerp(a, b, t);
    }
}