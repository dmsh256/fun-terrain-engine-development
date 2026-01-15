using UnityEngine;
using System;

namespace Service.WorldManagement.WorldSaver
{
    [Serializable]
    public class WorldObjectData
    {
        public int prefabId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}