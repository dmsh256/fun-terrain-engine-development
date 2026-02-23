using UnityEngine;

namespace Generators.HeightMap
{
    public interface IHeightMapModifier
    {
        Bounds bounds { get; }
        float Evaluate(float worldX, float worldZ, float currentHeight);
    }
}