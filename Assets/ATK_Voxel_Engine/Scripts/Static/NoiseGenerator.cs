using UnityEngine;

public static class NoiseGenerator
{
    // create a function that genereates a noise map
    public static int[,] GetNoiseMap(int seed, int size, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, float heightScale = 1)
    {
        // create a noise map
        int[,] noiseMap = new int[size, size];

        // create a random number generator with the seed
        System.Random prng = new System.Random(seed);

        // create an array of octaves
        Vector2[] octavesOffset = new Vector2[octaves];

        // create a max possible value for noise
        float maxPossibleNoiseValue = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octavesOffset[i] = new Vector2(offsetX, offsetY);

            maxPossibleNoiseValue += amplitude;
            amplitude *= persistance;
        }

        float halfWidth = size / 2.0f;
        float halfHeight = size / 2.0f;

        // loop through the noise map
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octavesOffset[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octavesOffset[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                noiseMap[x, y] = Mathf.FloorToInt(noiseHeight * heightScale);
            }
        }

        return noiseMap;
    }

    public static int[,,] GenerateCaveMap(int width, int height, int depth, Vector2 offset, float threshold = 1.0f, float scale = 1)
    {
        int[,,] noiseMap = new int[width, height * 2, depth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height * 2; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    float xCoord = (float)(x + offset.x) / width * scale;
                    float yCoord = (float)y / height * scale;
                    float zCoord = (float)(z + offset.y) / depth * scale;

                    float sample = Mathf.PerlinNoise(xCoord, zCoord) + Mathf.PerlinNoise(yCoord, zCoord);
                    noiseMap[x, y, z] = sample < threshold ? 1 : 0; // 1 for air, 0 for solid
                }
            }
        }

        return noiseMap;
    }
}