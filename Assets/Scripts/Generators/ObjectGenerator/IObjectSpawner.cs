using System.Collections.Generic;
using UnityEngine;

namespace Generators.ObjectGenerator
{
    public interface IObjectSpawner
    {
        void Spawn(ObjectSpawnContext context, List<GameObject> results);
    }
}