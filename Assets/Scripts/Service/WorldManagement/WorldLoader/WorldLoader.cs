using System;
using System.IO;
using Settings;
using UnityEngine;

namespace Service.WorldManagement.WorldLoader
{
    public class WorldLoader
    {
        private WorldSettings worldSettings;
        private static readonly string saveRoot = Application.dataPath;
        
        public static void LoadWorld(string worldName)
        {
            if (!File.Exists(GetFilePath(worldName)))
                throw new Exception("Cannot find world file: " + worldName);

            string json = File.ReadAllText(GetFilePath(worldName));
            
            JsonUtility.FromJson<WorldSettings>(json);
        }
        
        static string GetFilePath(string worldName)
        {
            return $"{saveRoot}/saves/{worldName}/world.meta";
        }
    }
}