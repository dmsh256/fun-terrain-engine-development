using System.Collections.Generic;

namespace WorldGeneration.WorldStructuralModifiers
{
    public class WorldStructure
    {
        public readonly List<IStructuralHeightModifier> structuralModifiers = new();
    }
}