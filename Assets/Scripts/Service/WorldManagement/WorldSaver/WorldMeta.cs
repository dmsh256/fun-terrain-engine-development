using System;
using UnityEngine;

namespace Service.WorldManagement.WorldSaver
{
    [Serializable]
    public class WorldMeta
    {
        public int seed;
        public string worldName;
        public Vector3 playerPosition;
    }
}