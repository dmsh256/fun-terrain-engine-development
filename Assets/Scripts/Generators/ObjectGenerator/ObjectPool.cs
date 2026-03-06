using System.Collections.Generic;
using Managers.Objects;
using UnityEngine;

namespace Generators.ObjectGenerator
{
    public class ObjectPool
    {
        private readonly GameObject prefab;
        private readonly Transform transformRoot;
        private readonly Queue<GameObject> objectPool;

        public ObjectPool(GameObject prefab, Transform poolRoot, int initialSize)
        {
            this.prefab = prefab;
            transformRoot = poolRoot;

            objectPool = new Queue<GameObject>(initialSize);
            for (int i = 0; i < initialSize; i++)
            {
                GameObject gameObject = Object.Instantiate(prefab, poolRoot);
                gameObject.SetActive(false);

                PooledObject pooled = gameObject.AddComponent<PooledObject>();
                pooled.Prefab = prefab;

                objectPool.Enqueue(gameObject);
            }
        }

        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (objectPool.Count == 0)
                Expand(32);

            GameObject gameObject = objectPool.Dequeue();
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);

            return gameObject;
        }

        private void Expand(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                
                GameObject gameObject = Object.Instantiate(prefab, transformRoot);
                PooledObject pooled = gameObject.AddComponent<PooledObject>();
                pooled.Prefab = prefab;
                gameObject.SetActive(false);
                objectPool.Enqueue(gameObject);
            }
        }
        
        public void Return(GameObject gameObject)
        {
            gameObject.SetActive(false);
            gameObject.transform.SetParent(transformRoot, false);
            objectPool.Enqueue(gameObject);
        }
    }
}
