using UnityEngine;

public static class SimplexNoise
{
    static int[] perm = new int[512];

    public static void Initialize(int seed)
    {
        int[] p = new int[256];
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < 256; i++)
            p[i] = i;

        for (int i = 0; i < 256; i++)
        {
            int swap = prng.Next(256);
            int temp = p[i];
            p[i] = p[swap];
            p[swap] = temp;
        }

        for (int i = 0; i < 512; i++)
            perm[i] = p[i & 255];
    }

    public static float Noise(float x, float y)
    {
        int xi = Mathf.FloorToInt(x) & 255;
        int yi = Mathf.FloorToInt(y) & 255;

        float xf = x - Mathf.Floor(x);
        float yf = y - Mathf.Floor(y);

        float u = Fade(xf);
        float v = Fade(yf);

        int aa = perm[perm[xi] + yi];
        int ab = perm[perm[xi] + yi + 1];
        int ba = perm[perm[xi + 1] + yi];
        int bb = perm[perm[xi + 1] + yi + 1];

        float x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
        float x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

        return (Lerp(x1, x2, v) + 1) * 0.5f;
    }

    static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    static float Lerp(float a, float b, float t) => a + t * (b - a);
    static float Grad(int hash, float x, float y)
    {
        int h = hash & 3;
        float u = h < 2 ? x : y;
        float v = h < 2 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}