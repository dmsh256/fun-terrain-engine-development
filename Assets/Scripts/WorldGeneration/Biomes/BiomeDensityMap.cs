namespace WorldGeneration.Biomes
{
    public class BiomeDensityMap
    {
        public int width;
        public int height;

        public int[,] primary;
        public int[,] secondary;

        public float[,] dominance;
        public int[,] borderDistance;
    }
}