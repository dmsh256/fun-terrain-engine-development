using System.Collections.Generic;
using Generators.ObjectGenerator;
using UnityEngine;

namespace Managers.Objects
{
    public class ObjectPoolManager
    {
        private const int poolInitialSize = 100;
        
        private readonly Dictionary<GameObject, ObjectPool> pools = new();
        private readonly Transform rootTransform;

        public ObjectPoolManager(Transform rootTransform)
        {
            this.rootTransform = rootTransform;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (!prefab)
                return null;
            
            if (!pools.TryGetValue(prefab, out ObjectPool pool))
            {
                pool = new ObjectPool(prefab, rootTransform, poolInitialSize);
                pools.Add(prefab, pool);
            }

            return pool.Get(position, rotation, parent);
        }

        public void Despawn(GameObject gameObject, GameObject prefab)
        {
            pools[prefab].Return(gameObject);
        }
    }
}