namespace Settings
{
    using UnityEngine;

    [CreateAssetMenu(
        fileName = "WorldSettings",
        menuName = "World/World Settings"
    )]
    public class WorldSettings : UpdatableData
    {
        [Header("Seed")]
        public int seed;
        public bool useRandomSeed;

        public string worldName;
    }
}