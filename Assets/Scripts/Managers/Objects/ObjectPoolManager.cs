using System.Collections.Generic;
using Generators.ObjectGenerator;
using UnityEngine;

namespace Managers.Objects
{
    public class ObjectPoolManager
    {
        private const int poolInitialSize = 4096;
        
        private readonly Dictionary<GameObject, ObjectPool> pools = new(32);
        private readonly Transform rootTransform;

        public ObjectPoolManager(Transform rootTransform)
        {
            this.rootTransform = rootTransform;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!prefab)
                return null;
            
            if (!pools.TryGetValue(prefab, out ObjectPool objectPool))
            {
                objectPool = new ObjectPool(prefab, rootTransform, poolInitialSize);
                pools.Add(prefab, objectPool);
            }

            return objectPool.Get(position, rotation);
        }

        public void Despawn(GameObject gameObject, GameObject prefab)
        {
            pools[prefab].Return(gameObject);
        }
    }
}