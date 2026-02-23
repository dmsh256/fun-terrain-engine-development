using System.Collections.Generic;
using Generators.ObjectGenerator;
using UnityEngine;
using WorldGeneration.ObjectDistributionStrategies;

namespace Generators.Nature
{
    public interface INatureObjectSpawner
    {
        IEnumerable<GameObject> Spawn(ObjectSpawnContext objectSpawnContext, IObjectDistributionStrategy objectDistributionStrategy,
            int seed, float spacing = 6f);
    }
}