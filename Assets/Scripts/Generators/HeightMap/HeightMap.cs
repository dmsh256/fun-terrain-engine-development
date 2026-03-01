namespace Generators.HeightMap
{
    public readonly struct HeightMap
    {
        public readonly float[,] values;
        private readonly float minValue;
        private readonly float maxValue;
        private readonly float heightMultiplier;
        
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
            return values[x, y];
        }

        public float getHeightMultiplier()
        {
            return heightMultiplier; 
        }
        
        public float getMinHeightValue()
        {
            return minValue; 
        }
        
        public float getMaxHeightValue()
        {
            return maxValue; 
        }
    }
}