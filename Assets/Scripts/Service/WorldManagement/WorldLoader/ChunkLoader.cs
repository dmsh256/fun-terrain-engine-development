using System;
using System.IO;
using Service.WorldManagement.WorldSaver;
using UnityEngine;

namespace Service.WorldManagement.WorldLoader
{
    class ChunkLoader
    {
        private static readonly string saveRoot = Application.dataPath;
        
        public static ChunkSaveData LoadChunk(Vector2Int coord)
        {
            string path = GetChunkPath(coord);
    
            if (!File.Exists(path))
                return null;
    
            string json = File.ReadAllText(path);
            
            return JsonUtility.FromJson<ChunkSaveData>(json);
        }
        
        static string GetChunkPath(Vector2Int coord)
        {
            String worldName = "UnknownWorld_" + new System.Random().Next(); // TODO get a real one
            
            return $"{saveRoot}/saves/{worldName}/chunks/{coord.x}_{coord.y}.chunk";
        }
    }
}