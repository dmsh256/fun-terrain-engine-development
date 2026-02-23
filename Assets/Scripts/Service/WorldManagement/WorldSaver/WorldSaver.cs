using System.IO;
using Settings;
using UnityEngine;

namespace Service.WorldManagement.WorldSaver
{
    public class WorldSaver
    {
        private static readonly string saveRoot = Application.dataPath;
        private static readonly WorldManager worldManager = WorldManager.Instance;

        public void saveWorld()
        {
            if (worldManager == null)
                throw 
                    new System.Exception("WorldManager not found");

            if (worldManager.name == null)
                throw 
                    new System.Exception("WorldManager name is null");

            if (worldManager.Seed == 0)
                throw
                    new System.Exception(
                        "WorldManager seed is null"); // TODO I don't know what would be the default value if it's not filled in the Unity Inspector

            string path = GetFilePath();
            string json = JsonUtility.ToJson(worldManager, true);

            File.WriteAllText(path, json); // rewrite? yeah
        }

        static string GetFilePath()
        {
            return $"{saveRoot}/saves/{worldManager.name}/world.meta";
        }
    }
}