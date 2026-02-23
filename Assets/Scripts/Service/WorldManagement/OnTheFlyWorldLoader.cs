using Generators.Terrain;
using UnityEngine;

namespace Service.WorldManagement
{
    public class OnTheFlyWorldLoader : MonoBehaviour
    {
        private static TerrainGenerator terrainGenerator;
        
        void Awake()
        {
            terrainGenerator = FindFirstObjectByType<TerrainGenerator>();
        }
        
        void Start()
        {
            terrainGenerator.Start();
        }

        void Update()
        {
            terrainGenerator.Update();
        }
    }
}