using Generators.HeightMap;
using Settings;

namespace WorldGeneration.WorldContextGenerator
{
    public class WorldContextGenerator
    {
        private readonly TerrainContextMapGenerator terrainContextGenerator;
        private readonly WorldSettings worldSettings;

        public WorldContextGenerator(WorldSettings worldSettings)
        {
            this.worldSettings = worldSettings;
            terrainContextGenerator = new TerrainContextMapGenerator(worldSettings);
        }
/*
        public WorldContext GenerateWorldContext(int resolution)
        {
            
        }*/
    }
}