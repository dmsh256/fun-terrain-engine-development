using Generators.HeightMap.HeightMapModifiers;
using Settings;
using WorldGeneration.WorldStructuralModifiers.LakeConnectionsModifier;

namespace WorldGeneration.WorldStructuralModifiers
{
    public class WorldStructureGenerator
    {
        public WorldStructure Generate(WorldSettings worldSettings, HeightMapSettings heightMapSettings, MeshSettings meshSettings, int resolution)
        {
            WorldStructure worldStructure = new ();
            
            LakeConnectionGenerator lakeConnectionGenerator = new(worldSettings, heightMapSettings, meshSettings);
            LakeStructuralModifierContext lakeStructuralModifierContext = lakeConnectionGenerator.GenerateWorldContext(resolution);

            foreach (LakeConnectionGenerator.CanyonPath canyonPath in lakeStructuralModifierContext.canyonPaths)
            {
                IStructuralHeightModifier canyonHeightModifier = new CanyonHeightModifier(
                    canyonPath, 
                    waterLevel: worldSettings.waterLevel
                );
                
                worldStructure.structuralModifiers.Add(canyonHeightModifier);
            }
            
            return worldStructure;
        }
    }
}