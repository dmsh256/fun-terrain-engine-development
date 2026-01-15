namespace Generators.Mask
{
    public class InvertMask : IMask
    {
        private readonly IMask source;
        public InvertMask(IMask source) => this.source = source;

        public float Sample(float x, float z)
            => 1f - source.Sample(x, z);
    }
}