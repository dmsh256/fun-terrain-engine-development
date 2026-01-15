using System.Collections.Generic;
using UnityEngine;

namespace Generators.ObjectGenerator
{
    public class VegetationSpawner : IObjectSpawner
    {
        private readonly GameObject[] prefabs;
        private readonly int count;
        private const float smallOffset = 0.5f;

        public VegetationSpawner(GameObject[] prefabs, int count)
        {
            this.prefabs = prefabs;
            this.count = count;
        }

        public void Spawn(ObjectSpawnContext context, List<GameObject> results)
        {
            if (prefabs.Length == 0) 
                return;

            for (int i = 0; i < count; i++)
            {
                GameObject obj = SpawnOne(context);
                if (obj)
                    results.Add(obj);
            }
        }

        private GameObject SpawnOne(ObjectSpawnContext context)
        {
            int x = Random.Range(0, (int) context.terrainChunk.MeshWorldSize());
            int z = Random.Range(0, (int) context.terrainChunk.MeshWorldSize());

            Vector2Int localPosition = new ();
            localPosition.x = x;
            localPosition.y = z;

            Vector3 worldPos = context.terrainChunk.WorldPosition();
            worldPos.x += x;
            worldPos.z += -z;

            float height = context.terrainChunk.SampleHeight(localPosition);
            worldPos.y = height - smallOffset;
            
            GameObject prefab = prefabs[context.random.Next(prefabs.Length)];
            prefab.name = "Veg at " + height;
            
            return Object.Instantiate(prefab, worldPos, Quaternion.identity, context.parent);
        }
    }
}