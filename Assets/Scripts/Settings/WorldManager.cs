namespace Settings
{
    using UnityEngine;

    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance;

        public WorldSettings worldSettings;

        public int Seed { get; set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Seed = worldSettings.useRandomSeed
                ? Random.Range(int.MinValue, int.MaxValue)
                : worldSettings.seed;

            Random.InitState(Seed);
        }
    }
}