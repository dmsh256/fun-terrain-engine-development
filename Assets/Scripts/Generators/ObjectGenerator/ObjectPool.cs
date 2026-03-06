using System.Collections.Generic;
using Managers.Objects;
using UnityEngine;

namespace Generators.ObjectGenerator
{
    public class ObjectPool
    {
        private readonly GameObject prefab;
        private readonly Transform transformRoot;
        private readonly Queue<GameObject> objectPool = new();

        public ObjectPool(GameObject prefab, Transform transformRoot, int initialSize)
        {
            this.prefab = prefab;
            this.transformRoot = transformRoot;

            for (int i = 0; i < initialSize; i++)
            {
                GameObject gameObject = Object.Instantiate(prefab, transformRoot);
                gameObject.SetActive(false);
                objectPool.Enqueue(gameObject);
            }
        }

        public GameObject Get(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (objectPool.Count == 0)
            {
                Expand(10);
            }
            
            GameObject gameObject;
            if (objectPool.Count > 0)
            {
                gameObject = objectPool.Dequeue();
                gameObject.transform.SetParent(parent);
                gameObject.transform.SetPositionAndRotation(position, rotation);
                gameObject.SetActive(true);
            }
            else
            {
                gameObject = Object.Instantiate(prefab, position, rotation, parent);

                PooledObject pooled = gameObject.GetComponent<PooledObject>();
                if (!pooled)
                    pooled = gameObject.AddComponent<PooledObject>();

                pooled.Prefab = prefab;
            }

            return gameObject;
        }

        private void Expand(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject go = Object.Instantiate(prefab, transformRoot);
                go.SetActive(false);

                PooledObject pooled = go.GetComponent<PooledObject>();
                if (!pooled)
                    pooled = go.AddComponent<PooledObject>();

                pooled.Prefab = prefab;

                objectPool.Enqueue(go);
            }
        }
        
        public void Return(GameObject gameObject)
        {
            gameObject.SetActive(false);
            gameObject.transform.SetParent(transformRoot);
            objectPool.Enqueue(gameObject);
        }
    }
}
