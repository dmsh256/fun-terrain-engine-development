using System.Collections.Generic;
using UnityEngine;

namespace Service.WorldManagement.WorldSaver
{
    public class Chunk
    {
        public Vector2Int coord;
        public bool isDirty;

        public List<HeightDelta> heightDeltas = new();
        public List<WorldObjectData> placedObjects = new();
    }
}