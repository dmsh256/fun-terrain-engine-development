using Generators;
using UnityEngine;

namespace Service.WorldManagement
{
    public class OnTheFlyWorldLoader : MonoBehaviour
    {
        private new GameObject gameObject;
        private static OnTheFlyTerrainGenerator _onTheFlyTerrainGenerator;
        
        void Awake()
        {
            //gameObject = new GameObject("OnTheFlyWorldLoader");
            //_onTheFlyTerrainGenerator = gameObject.AddComponent<OnTheFlyTerrainGenerator>();
            _onTheFlyTerrainGenerator = FindFirstObjectByType<OnTheFlyTerrainGenerator>();
        }
        
        void Start()
        {
            _onTheFlyTerrainGenerator.Start();
        }

        void Update()
        {
            _onTheFlyTerrainGenerator.Update();
        }
    }
}