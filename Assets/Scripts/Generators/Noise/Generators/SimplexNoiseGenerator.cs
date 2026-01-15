namespace Generators.Noise.Generators
{
    using UnityEngine;

    public class SimplexNoiseGenerator : INoiseGenerator
    {
        private static readonly int[][] grad2 =
        {
            new[] { 1, 1 }, new[] { -1, 1 }, new[] { 1, -1 }, new[] { -1, -1 },
            new[] { 1, 0 }, new[] { -1, 0 }, new[] { 0, 1 }, new[] { 0, -1 }
        };

        private static int[] BuildPerm(int seed)
        {
            int[] p = new int[256];
            for (int i = 0; i < 256; i++) p[i] = i;

            System.Random rng = new System.Random(seed);
            for (int i = 255; i > 0; i--)
            {
                int swap = rng.Next(i + 1);
                (p[i], p[swap]) = (p[swap], p[i]);
            }

            int[] perm = new int[512];
            for (int i = 0; i < 512; i++)
                perm[i] = p[i & 255];

            return perm;
        }

        private static float Dot(int[] g, float x, float y)
        {
            return g[0] * x + g[1] * y;
        }

        private static float Simplex(float xin, float yin, int[] perm)
        {
            const float F2 = 0.366025403f;
            const float G2 = 0.211324865f;

            float s = (xin + yin) * F2;
            int i = Mathf.FloorToInt(xin + s);
            int j = Mathf.FloorToInt(yin + s);

            float t = (i + j) * G2;
            float X0 = i - t;
            float Y0 = j - t;

            float x0 = xin - X0;
            float y0 = yin - Y0;

            int i1, j1;
            if (x0 > y0) { i1 = 1; j1 = 0; }
            else { i1 = 0; j1 = 1; }

            float x1 = x0 - i1 + G2;
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1f + 2f * G2;
            float y2 = y0 - 1f + 2f * G2;

            int ii = i & 255;
            int jj = j & 255;

            float n0 = 0, n1 = 0, n2 = 0;

            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 >= 0)
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(grad2[perm[ii + perm[jj]] % 8], x0, y0);
            }

            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 >= 0)
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(grad2[perm[ii + i1 + perm[jj + j1]] % 8], x1, y1);
            }

            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 >= 0)
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(grad2[perm[ii + 1 + perm[jj + 1]] % 8], x2, y2);
            }

            return 70f * (n0 + n1 + n2); // approx [-1..1]
        }

        public static float[,] GenerateNoiseMap(
            int width,
            int height,
            NoiseSettings settings,
            Vector2 sampleCentre)
        {
            int[] perm = BuildPerm(settings.seed);

            float[,] map = new float[width, height];

            float halfW = width / 2f;
            float halfH = height / 2f;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float nx =
                        (x - halfW + sampleCentre.x + settings.offset.x) / settings.scale;
                    float ny =
                        (y - halfH - sampleCentre.y + settings.offset.y) / settings.scale;

                    float value = Simplex(nx, ny, perm);
                    
                    map[x, y] = value * 0.5f + 0.5f;
                }

            return map;
        }
    }
}