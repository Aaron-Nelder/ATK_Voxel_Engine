using UnityEngine;

public class NoiseMapGenerator : MonoBehaviour
{
    public ComputeShader perlinNoiseComputeShader;
    public int width = 512;
    public int height = 512;
    public float scale = 20.0f;
    public int offsetX = 0;
    public int offsetY = 0;
    public float amplitude = 1.0f;
    public float frequency = 0.1f;
    public int octaves = 8;
    public float lacunarity = 2.0f;
    public float persistence = 0.5f;
    public int seed = 42;
    public int maxValue = 255;

    private int[] noiseData;
    private ComputeBuffer noiseBuffer;

    public int[,] GetNoiseMap(int seed, int size, float scale, int octaves, float amplitude, float frequency, float persistence, float lacunarity, Vector2Int offset, int maxValue = 1)
    {
        noiseData = new int[size * size];
        noiseBuffer = new ComputeBuffer(noiseData.Length, sizeof(int));

        // Wrap the offsets to ensure they are within the range [0, 256)
        offset.x = (offset.x % 256 + 256) % 256;
        offset.y = (offset.y % 256 + 256) % 256;

        perlinNoiseComputeShader.SetBuffer(0, "Result", noiseBuffer);
        perlinNoiseComputeShader.SetFloat("scale", scale);
        perlinNoiseComputeShader.SetInt("offsetX", offset.x);
        perlinNoiseComputeShader.SetInt("offsetY", offset.y);
        perlinNoiseComputeShader.SetFloat("amplitude", amplitude);
        perlinNoiseComputeShader.SetFloat("frequency", frequency);
        perlinNoiseComputeShader.SetInt("octaves", octaves);
        perlinNoiseComputeShader.SetFloat("lacunarity", lacunarity);
        perlinNoiseComputeShader.SetFloat("persistence", persistence);
        perlinNoiseComputeShader.SetInt("seed", seed);
        perlinNoiseComputeShader.SetInt("maxValue", maxValue);

        int threadGroupsX = Mathf.CeilToInt(size / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(size / 8.0f);
        perlinNoiseComputeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        noiseBuffer.GetData(noiseData);
        noiseBuffer.Release();

        // Convert the 1D array to a 2D array for easier use
        int[,] noiseMap = new int[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                noiseMap[x, y] = noiseData[y * size + x];
            }
        }
        return noiseMap;
    }

    /*
    void Start()
    {
        noiseData = new int[width * height];
        noiseBuffer = new ComputeBuffer(noiseData.Length, sizeof(int));

        perlinNoiseComputeShader.SetBuffer(0, "Result", noiseBuffer);
        perlinNoiseComputeShader.SetFloat("scale", scale);
        perlinNoiseComputeShader.SetFloat("offsetX", offsetX);
        perlinNoiseComputeShader.SetFloat("offsetY", offsetY);
        perlinNoiseComputeShader.SetFloat("amplitude", amplitude);
        perlinNoiseComputeShader.SetFloat("frequency", frequency);
        perlinNoiseComputeShader.SetInt("octaves", octaves);
        perlinNoiseComputeShader.SetFloat("lacunarity", lacunarity);
        perlinNoiseComputeShader.SetFloat("persistence", persistence);
        perlinNoiseComputeShader.SetInt("seed", seed);
        perlinNoiseComputeShader.SetInt("maxValue", maxValue);

        int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
        perlinNoiseComputeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        noiseBuffer.GetData(noiseData);
        noiseBuffer.Release();

        // Convert the 1D array to a 2D array for easier use
        int[,] noiseMap = new int[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = noiseData[y * width + x];
                Debug.Log(noiseMap[x, y]);
            }
        }
    }
    */
}