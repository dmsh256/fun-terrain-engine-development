using UnityEngine;

namespace WorldGeneration.WorldStructuralModifiers
{
    public interface IStructuralHeightModifier
    {
        public float Evaluate(float worldX, float worldZ, float height);
        
        public Bounds bounds { get; } 
    }
}