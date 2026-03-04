using System.Collections.Generic;

namespace WorldGeneration.WorldStructuralModifiers.MountainModifier
{
    public class MountainStructuralContext
    {
        public readonly List<MountainGenerator.PeakCluster> clusters = new();
    }
}