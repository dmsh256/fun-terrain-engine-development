using Settings.Biome;
using UnityEngine;

namespace WorldGeneration.Biomes
{
    public static class BiomeAlbedoArrayBuilder
    {
        private const int TexturesPerBiome = 3; // grass + soil + rock

        public static Texture2DArray Build(BiomeData[] biomes)
        {
            Texture2D first = biomes[0].grassTexture;

            int size = first.width;
            int count = biomes.Length * TexturesPerBiome;
            TextureFormat format = first.format;
            bool mipmaps = first.mipmapCount > 1;

            Texture2DArray array = new(size, size, count, format, mipmaps);

            for (int biome = 0; biome < biomes.Length; biome++)
            {
                int baseIndex = biome * TexturesPerBiome;

                CopyTexture(biomes[biome].soilTexture,  baseIndex + 0);
                CopyTexture(biomes[biome].grassTexture, baseIndex + 1);
                CopyTexture(biomes[biome].rockTexture,  baseIndex + 2);
            }

            array.wrapMode = TextureWrapMode.Repeat;
            array.filterMode = FilterMode.Bilinear;
            array.Apply(false, true);

            return array;

            void CopyTexture(Texture2D texture, int slice)
            {
                if (!texture)
                {
                    Debug.LogError("Missing biome texture");
                    return;
                }

                if (texture.width != size || texture.height != size)
                {
                    Debug.LogError(
                        $"Biome size mismatch: expected {size}x{size}, got {texture.width}x{texture.height}"
                    );
                    return;
                }

                if (texture.format != format)
                {
                    Debug.LogError(
                        $"Biome albedo mismatch: expected {format}, got {texture.format}"
                    );
                    return;
                }

                for (int mip = 0; mip < texture.mipmapCount; mip++)
                {
                    Graphics.CopyTexture(texture, 0, mip, array, slice, mip);
                }
            }
        }
    }
}