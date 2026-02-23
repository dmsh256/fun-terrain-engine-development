namespace Generators.BiomeMap.BiomeTerrainShapers
{
    public class BiomeTerrainZeroShaper : BiomeTerrainShaper
    {
        public override float Shape(TerrainContext terrainContext)
        {
            return 0f;
        }
    }
}