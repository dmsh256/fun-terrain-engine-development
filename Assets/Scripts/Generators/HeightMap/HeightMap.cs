namespace Generators.HeightMap
{
    public readonly struct HeightMap
    {
        public readonly float[,] values;
        public readonly float minValue;
        public readonly float maxValue;
        public readonly float heightMultiplier;
        
        public HeightMap(float[,] values, float minValue, float maxValue, float heightMultiplier = 1f)
        {
            this.values = values;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.heightMultiplier = heightMultiplier;
        }
        
        public float getHeight(int x, int y)
        {
            return values[x, y] * heightMultiplier;
        }
        
        public float getRawHeight(int x, int y)
        {
            return values[x, y] / heightMultiplier;
        }
    }
}