using System;

namespace Service.WorldManagement.WorldSaver
{
    [Serializable]
    public struct HeightDelta
    {
        public byte x;
        public byte z;
        public float delta;
    }
}