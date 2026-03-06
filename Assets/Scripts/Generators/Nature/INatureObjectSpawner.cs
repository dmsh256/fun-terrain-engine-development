using System.Collections.Generic;
using Generators.ObjectGenerator;
using UnityEngine;
using WorldGeneration.ObjectDistributionStrategies;

namespace Generators.Nature
{
    public interface INatureObjectSpawner
    {
        void Spawn(ObjectSpawnContext objectSpawnContext, IObjectDistributionStrategy objectDistributionStrategy,
            int seed, System.Action<GameObject> emit, float spacing = 6f);
    }
}