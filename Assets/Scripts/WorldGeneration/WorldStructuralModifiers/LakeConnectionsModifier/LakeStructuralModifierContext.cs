using System.Collections.Generic;

namespace WorldGeneration.WorldStructuralModifiers.LakeConnectionsModifier
{
    public class LakeStructuralModifierContext
    {
        public float trenchWidth;
        
        public List<LakeConnectionGenerator.LakeData> lakes;
        public List<LakeConnectionGenerator.LakeConnection> lakeConnections;
        public List<LakeConnectionGenerator.CanyonPath> canyonPaths;
    }
}