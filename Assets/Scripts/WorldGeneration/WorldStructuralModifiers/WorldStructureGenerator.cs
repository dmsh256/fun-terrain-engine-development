using Generators.HeightMap.HeightMapModifiers.StructuralModifiers;
using Settings;
using WorldGeneration.WorldStructuralModifiers.LakeConnectionsModifier;
using WorldGeneration.WorldStructuralModifiers.MountainModifier;

namespace WorldGeneration.WorldStructuralModifiers
{
    public class WorldStructureGenerator
    {
        public WorldStructure Generate(WorldSettings worldSettings, HeightMapSettings heightMapSettings, MeshSettings meshSettings, int resolution)
        {
            WorldStructure worldStructure = new ();

            LakeStructuralModifierContext lakeStructuralModifierContext = null;
            
            if (worldSettings.canyonSettings.canyonsBetweenLakesEnabled)
            {
                LakeConnectionGenerator lakeConnectionGenerator = new(worldSettings, heightMapSettings, meshSettings);
                lakeStructuralModifierContext = lakeConnectionGenerator.GenerateContext(resolution);
    
                foreach (LakeConnectionGenerator.CanyonPath canyonPath in lakeStructuralModifierContext.canyonPaths)
                {
                    IStructuralHeightModifier canyonHeightModifier = new CanyonHeightModifier(
                        canyonPath, 
                        waterLevel: worldSettings.waterLevel
                    );
                    
                    worldStructure.structuralModifiers.Add(canyonHeightModifier);
                }
            }

            if (worldSettings.mountainsSettings.mountainsEnabled)
            {
                MountainGenerator mountainGenerator = new(worldSettings, heightMapSettings, meshSettings);
                MountainStructuralContext mountainContext = mountainGenerator.GenerateContext(resolution, lakeStructuralModifierContext);
                
                foreach (MountainGenerator.PeakCluster cluster in mountainContext.clusters)
                {
                    worldStructure.structuralModifiers.Add(
                        new MountainHeightModifier(
                            cluster,
                            worldSettings.mountainsSettings.mountainPeakRadius,
                            worldSettings.mountainsSettings.mountainRidgeWidth,
                            worldSettings.mountainsSettings.amplificationStrength
                        )
                    );
                }
            }

            return worldStructure;
        }
    }
}