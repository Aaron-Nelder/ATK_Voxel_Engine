using UnityEngine;

namespace ATKVoxelEngine
{
    public static class NoiseGenerator
    {
        public static int[,] GetNoise2D(WorldSettings_SO wSettings, NoiseProfile_SO pro, int xOffset, int yOffset)
        {
            // create a noise map
            int[,] noiseMap = new int[wSettings.ChunkSize.x, wSettings.ChunkSize.z];

            // create a random number generator with the seed
            System.Random prng = new System.Random(wSettings.Seed);

            // create an array of octaves
            Vector2[] octavesOffset = new Vector2[pro.Octaves];

            // create a max possible value for noise
            float maxPossibleNoiseValue = 0;
            float amplitude = 1;
            float frequency = 1;

            for (int i = 0; i < pro.Octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + xOffset;
                float offsetY = prng.Next(-100000, 100000) + yOffset;
                octavesOffset[i] = new Vector2(offsetX, offsetY);

                maxPossibleNoiseValue += amplitude;
                amplitude *= pro.Amplitude;
            }

            float halfWidth = wSettings.ChunkSize.x / 2.0f;
            float halfHeight = wSettings.ChunkSize.z / 2.0f;

            // loop through the noise map
            for (int x = 0; x < wSettings.ChunkSize.x; x++)
            {
                for (int y = 0; y < wSettings.ChunkSize.z; y++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < pro.Octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octavesOffset[i].x) / pro.Scale * frequency;
                        float sampleY = (y - halfHeight + octavesOffset[i].y) / pro.Scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= pro.Amplitude;
                        frequency *= pro.Frequency;
                    }

                    // Normalize the noiseHeight to be within the range of 0 to 1
                    noiseHeight = (noiseHeight + maxPossibleNoiseValue) / (2 * maxPossibleNoiseValue);
                    noiseMap[x, y] = Mathf.FloorToInt(noiseHeight * pro.MagClamp);
                }
            }

            return noiseMap;
        }

        // Generates a 3D noise map for caves
        public static int[,,] GetNoise3D(WorldSettings_SO wSettings, NoiseProfile_SO pro, int xOffset, int yOffset)
        {           
            // create a noise map
            int[,,] noiseMap = new int[wSettings.ChunkSize.x, wSettings.ChunkSize.y, wSettings.ChunkSize.z];

            // create a random number generator with the seed
            System.Random prng = new System.Random(wSettings.Seed);

            // create an array of octaves
            Vector3[] octavesOffset = new Vector3[pro.Octaves];

            // create a max possible value for noise
            float maxPossibleNoiseValue = 0;
            float amplitude = 1;
            float frequency = 1;


            for (int i = 0; i < pro.Octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + xOffset;
                float offsetY = prng.Next(-100000, 100000) + yOffset;
                float offsetZ = prng.Next(-100000, 100000);
                octavesOffset[i] = new Vector3(offsetX, offsetY, offsetZ);

                maxPossibleNoiseValue += amplitude;
                amplitude *= pro.Amplitude;
            }

            for (int x = 0; x < wSettings.ChunkSize.x; x++)
            {
                for (int y = 0; y < wSettings.ChunkSize.y; y++)
                {
                    for (int z = 0; z < wSettings.ChunkSize.z; z++)
                    {
                        amplitude = 1;
                        frequency = 1;
                        float noiseValue = 0;

                        for (int i = 0; i < pro.Octaves; i++)
                        {
                            float sampleX = (x + octavesOffset[i].x) / pro.Scale * frequency;
                            float sampleY = (y + octavesOffset[i].y) / pro.Scale * frequency;
                            float sampleZ = (z + octavesOffset[i].z) / pro.Scale * frequency;

                            float perlinValue = Perlin3D(sampleX, sampleY, sampleZ) * 2 - 1;
                            noiseValue += perlinValue * amplitude;

                            amplitude *= pro.Amplitude;
                            frequency *= pro.Frequency;
                        }

                        noiseValue = (noiseValue + maxPossibleNoiseValue) / (2 * maxPossibleNoiseValue);
                        noiseValue *= pro.MagClamp;

                        //Debug.Log(noiseValue);

                        if (pro.UseThreshold)
                            noiseMap[x, y, z] = noiseValue >= pro.Threshold.x && noiseValue <= pro.Threshold.y ? 1 : 0;
                        else
                            noiseMap[x, y, z] = noiseValue >= 0.5f ? 1 : 0;
                    }
                }
            }
            return noiseMap;
            
        }
        static float Perlin3D(float x, float y, float z)
        {
            float ab = Mathf.PerlinNoise(x, y);
            float bc = Mathf.PerlinNoise(x, z);
            float ac = Mathf.PerlinNoise(y, z);
            float ba = Mathf.PerlinNoise(y, x);
            float cb = Mathf.PerlinNoise(z, x);
            float ca = Mathf.PerlinNoise(z, y);

            return (ab + bc + ac + ba + cb + ca) / 6.0f;
        }
    }
}