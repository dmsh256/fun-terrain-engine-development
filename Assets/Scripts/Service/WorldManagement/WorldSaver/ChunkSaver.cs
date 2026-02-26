using System.IO;
using UnityEngine;

namespace Service.WorldManagement.WorldSaver
{
    // TODO use actual
    public class ChunkSaver
    {
        private static readonly string saveRoot = Application.dataPath;
        private static string worldName = "tmp";
        
        public static void SaveChunk(Chunk chunk)
        {
            if (!chunk.isDirty)
                return;

            ChunkSaveData data = new ()
            {
                x = chunk.coord.x,
                y = chunk.coord.y,
                heightDeltas = chunk.heightDeltas,
                objects = chunk.placedObjects
            };

            string path = GetChunkPath(chunk.coord);
            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(path, json);
            chunk.isDirty = false;
        }

        private static string GetChunkPath(Vector2Int coord)
        {
            worldName ??= "UnknownWorld_" + new System.Random().Next(); // collision possibility, but to hell with that :)

            return $"{saveRoot}/saves/{worldName}/chunks/{coord.x}_{coord.y}.chunk";
        }
    }
}