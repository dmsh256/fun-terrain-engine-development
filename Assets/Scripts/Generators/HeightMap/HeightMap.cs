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
        
        public float GetHeight(int x, int y)
        {
            return values[x, y] * heightMultiplier;
        }
        
        public float GetRawHeight(int x, int y)
        {
            return values[x, y];
        }

        public float GetHeightMultiplier()
        {
            return heightMultiplier; 
        }
        
        public float GetMinHeightValue()
        {
            return minValue; 
        }
        
        public float GetMaxHeightValue()
        {
            return maxValue; 
        }
    }
}