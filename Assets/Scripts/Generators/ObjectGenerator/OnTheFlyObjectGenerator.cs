using System.Collections.Generic;
using Settings;
using UnityEngine;

namespace Generators.ObjectGenerator
{
    public class OnTheFlyObjectGenerator : MonoBehaviour
    {
        public int worldSeed;
        public GameObject[] treePrefabs;
        public GameObject[] rockPrefabs;
        public GameObject[] vegetationPrefabs;

        private readonly Dictionary<Vector2, ObjectChunk> objectChunks = new();

        public void Init()
        {
            worldSeed = WorldManager.Instance.Seed;
        }

        public void OnChunkVisibilityChanged(TerrainChunk terrainChunk, bool visible)
        {
            if (visible)
            {
                if (objectChunks.ContainsKey(terrainChunk.coord))
                    return;

                ObjectChunk chunk = new(
                    terrainChunk,
                    worldSeed,
                    treePrefabs,
                    rockPrefabs,
                    vegetationPrefabs,
                    transform
                );

                objectChunks.Add(terrainChunk.coord, chunk);
                chunk.Spawn();
            }
            else
            {
                if (!objectChunks.TryGetValue(terrainChunk.coord, out ObjectChunk chunk)) 
                    return;
                
                chunk.Despawn();
                objectChunks.Remove(terrainChunk.coord);
            }
        }
    }
}