using System;
using System.Collections.Generic;

namespace Service.WorldManagement.WorldSaver
{
    [Serializable]
    public class ChunkSaveData
    {
        public int x;
        public int y;

        public List<HeightDelta> heightDeltas = new();
        public List<WorldObjectData> objects = new();
    }
}