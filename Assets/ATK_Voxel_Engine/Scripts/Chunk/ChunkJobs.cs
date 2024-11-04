using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using ATKVoxelEngine;
using UnityEngine.UIElements;

[BurstCompile]
public struct AssignVoxelsJob : IJob
{
    NativeArray<uint> _voxels;
    [ReadOnly] NativeArray<int> _heightNoise;
    [ReadOnly] NativeArray<int> _caveNoise;
    readonly int3 _chunkSize;

    public AssignVoxelsJob(ref NativeArray<uint> voxels, ref NativeArray<int> heightNoise, ref NativeArray<int> caveNoise, int3 chunkSize)
    {
        _voxels = voxels;
        _heightNoise = heightNoise;
        _caveNoise = caveNoise;
        _chunkSize = chunkSize;
    }

    public void Execute()
    {
        // loop through the x and z axis
        for (int x = 0; x < _chunkSize.x; x++)
        {
            for (int z = 0; z < _chunkSize.z; z++)
            {
                int height = _heightNoise[x + (z * _chunkSize.x)];
                // loop through the y axis
                for (int y = height; y >= 0; y--)
                {
                    int index3D = PosToIndex3D(x, y, z);
                    _voxels[index3D] = SetVoxel(new(x, y, z), height);
                }
            }
        }
    }

    uint SetVoxel(int3 pos, int surfaceHeight)
    {
        bool isVoxel = _caveNoise[PosToIndex3D(pos.x, pos.y, pos.z)] == 1;

        // BEDROCK
        if (pos.y == 0)
            return 2;

        // Top Soil
        if (surfaceHeight - pos.y < 3 && isVoxel)
            return 1;

        // Underground
        return (uint)(isVoxel ? 2 : 0);
    }

    int PosToIndex3D(int x, int y, int z) => x + y * _chunkSize.x + z * _chunkSize.x * _chunkSize.y;
}

[BurstCompile]
public struct GetNoise2DJob : IJob
{
    int2 _noiseSize, _offset;
    float _amplitude, _frequency;
    int _seed, _magClamp;
    float2 _scale;
    ushort _octaves;

    NativeArray<int> _noiseMap;
    NativeArray<float> _octavesOffset;

    public GetNoise2DJob(WorldSettings_SO wSettings, NoiseProfile_SO noiseSettings, int3 wPos, ref NativeArray<int> noise, ref NativeArray<float> octavesOffset)
    {
        _noiseSize = new int2(wSettings.ChunkSize.x, wSettings.ChunkSize.z);
        _scale = noiseSettings.Scale;
        _seed = wSettings.Seed;
        _octaves = (ushort)noiseSettings.Octaves;
        _amplitude = noiseSettings.Amplitude;
        _magClamp = noiseSettings.MagClamp;
        _frequency = noiseSettings.Frequency;
        _offset.x = wPos.x;
        _offset.y = wPos.z;

        _noiseMap = noise;
        _octavesOffset = octavesOffset;
    }

    public void Execute()
    {
        Random random = new Random((uint)_seed);

        // create a max possible value for noise
        float maxPossibleNoiseValue = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < _octavesOffset.Length; i += 2)
        {
            _octavesOffset[i] = random.NextFloat(-100000, 100000) + _offset.x;
            _octavesOffset[i + 1] = random.NextFloat(-100000, 100000) + _offset.y;

            maxPossibleNoiseValue += amplitude;
            amplitude *= _amplitude;
        }

        float halfWidth = _noiseSize.x / 2.0f;
        float halfHeight = _noiseSize.y / 2.0f;

        // loop through the noise map
        for (int x = 0; x < _noiseSize.x; x++)
        {
            for (int y = 0; y < _noiseSize.y; y++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < _octavesOffset.Length; i += 2)
                {
                    float sampleX = (x - halfWidth + _octavesOffset[i]) / _scale.x * frequency;
                    float sampleY = (y - halfHeight + _octavesOffset[i + 1]) / _scale.y * frequency;

                    float value = noise.pnoise(new float2(sampleX, sampleY), new float2(sampleX, sampleY) * 2 - 1);

                    //float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += value * amplitude;

                    amplitude *= _amplitude;
                    frequency *= _frequency;
                }

                // Normalize the noiseHeight to be within the range of 0 to 1
                noiseHeight = (noiseHeight + maxPossibleNoiseValue) / (2 * maxPossibleNoiseValue);
                _noiseMap[x + y * _noiseSize.x] = (ushort)math.floor(noiseHeight * _magClamp);
            }
        }
    }
}

[BurstCompile]
public struct GetNoise3DJob : IJob
{
    int3 _noiseSize;
    float3 _scale;
    int _seed;
    ushort _octaves;
    float _amplitude;
    float _frequency;
    int3 _offset;
    float2 _caveThreshold;

    NativeArray<int> _noiseMap;
    NativeArray<float> _octavesOffset;

    public GetNoise3DJob(WorldSettings_SO settings, NoiseProfile_SO noiseSettings, int3 wPos, ref NativeArray<int> _noise, ref NativeArray<float> octavesOffset)
    {
        _noiseSize = settings.ChunkSize;
        _scale = noiseSettings.Scale;
        _seed = settings.Seed;
        _octaves = (ushort)noiseSettings.Octaves;
        _amplitude = noiseSettings.Amplitude;
        _frequency = noiseSettings.Frequency;
        _offset = wPos;
        _caveThreshold = noiseSettings.Threshold;

        _noiseMap = _noise;
        _octavesOffset = octavesOffset;
    }

    public void Execute()
    {
        Random random = new Random((uint)_seed);

        // create a max possible value for noise
        float maxPossibleNoiseValue = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < _octavesOffset.Length; i += 3)
        {
            _octavesOffset[i] = random.NextFloat(-100000, 100000) + _offset.x;
            _octavesOffset[i + 1] = random.NextFloat(-100000, 100000) + _offset.y;
            _octavesOffset[i + 2] = random.NextFloat(-100000, 100000) + _offset.z;

            maxPossibleNoiseValue += amplitude;
            amplitude *= _amplitude;
        }

        // loop through the noise map
        for (int x = 0; x < _noiseSize.x; x++)
        {
            for (int y = 0; y < _noiseSize.y; y++)
            {
                for (int z = 0; z < _noiseSize.z; z++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseValue = 0;

                    for (int i = 0; i < _octavesOffset.Length; i += 3)
                    {
                        float sampleX = (x + _octavesOffset[i]) / _scale.x * frequency;
                        float sampleY = (y + _octavesOffset[i + 1]) / _scale.y * frequency;
                        float sampleZ = (z + _octavesOffset[i + 2]) / _scale.z * frequency;
                        float value = noise.snoise(new float3(sampleX, sampleY, sampleZ));
                        noiseValue += value * amplitude;
                        amplitude *= _amplitude;
                        frequency *= _frequency;
                    }

                    // Normalize the noiseValue to be within the range of 0 to 1
                    noiseValue = (noiseValue + maxPossibleNoiseValue) / (2 * maxPossibleNoiseValue);

                    // if the noiseValue is below the caveThreshold, set the voxel to air (0)
                    if (noiseValue < _caveThreshold.x || noiseValue > _caveThreshold.y)
                        _noiseMap[x + y * _noiseSize.x + z * _noiseSize.x * _noiseSize.y] = 0;
                    else
                        _noiseMap[x + y * _noiseSize.x + z * _noiseSize.x * _noiseSize.y] = 1;
                }
            }
        }
    }
}

[BurstCompile]
public struct GenerateTerrainJob : IJob
{
    NativeArray<uint> _voxels;
    readonly int3 _chunkSize;
    int _seed;
    int3 _offset;

    int2 _hNoiseSize;
    float _hNoiseAmplitude, _hNoiseFrequency;
    int _hNoiseMagClamp;
    float2 _hNoiseScale;
    ushort _hNoiseOctaves;
    NativeArray<int> _hNoiseMap;
    NativeArray<float> _hNoiseOctavesOffset;

    int3 _cNoiseSize;
    float3 _cNoiseScale;
    ushort _cNoiseOctaves;
    float _cNoiseAmplitude, _cNoiseFrequency;
    float2 _cNoiseThreshold;
    NativeArray<int> _cNoiseMap;
    NativeArray<float> _cNoiseOctavesOffset;

    public GenerateTerrainJob(ref NativeArray<uint> voxels, WorldSettings_SO wSettings, int3 wPos, ref NativeArray<int> hNoiseMap, ref NativeArray<float> hNoiseOctavesOffset, ref NativeArray<int> cNoiseMap, ref NativeArray<float> cNoiseOctavesOffset)
    {
        _voxels = voxels;
        _chunkSize = wSettings.ChunkSize;
        _seed = wSettings.Seed;
        _offset = wPos;

        _hNoiseSize = new int2(wSettings.ChunkSize.x, wSettings.ChunkSize.z);
        _hNoiseScale = wSettings.HeightNoise.Scale;
        _hNoiseOctaves = (ushort)wSettings.HeightNoise.Octaves;
        _hNoiseAmplitude = wSettings.HeightNoise.Amplitude;
        _hNoiseFrequency = wSettings.HeightNoise.Frequency;
        _hNoiseMagClamp = wSettings.HeightNoise.MagClamp;
        _hNoiseMap = hNoiseMap;
        _hNoiseOctavesOffset = hNoiseOctavesOffset;

        _cNoiseSize = wSettings.ChunkSize;
        _cNoiseScale = wSettings.CaveNoise.Scale;
        _cNoiseOctaves = (ushort)wSettings.CaveNoise.Octaves;
        _cNoiseAmplitude = wSettings.CaveNoise.Amplitude;
        _cNoiseFrequency = wSettings.CaveNoise.Frequency;
        _cNoiseThreshold = wSettings.CaveNoise.Threshold;
        _cNoiseMap = cNoiseMap;
        _cNoiseOctavesOffset = cNoiseOctavesOffset;
    }

    public void Execute()
    {
        Random random = new Random((uint)_seed);
        GenerateHeightNoise(ref random);
        GenerateCaveNoise(ref random);
        AssignVoxels();
    }

    void GenerateHeightNoise(ref Random random)
    {
        // create a max possible value for noise
        float maxPossibleNoiseValue = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < _hNoiseOctavesOffset.Length; i += 2)
        {
            _hNoiseOctavesOffset[i] = random.NextFloat(-100000, 100000) + _offset.x;
            _hNoiseOctavesOffset[i + 1] = random.NextFloat(-100000, 100000) + _offset.z;

            maxPossibleNoiseValue += amplitude;
            amplitude *= _hNoiseAmplitude;
        }

        float halfWidth = _hNoiseSize.x / 2.0f;
        float halfHeight = _hNoiseSize.y / 2.0f;

        // loop through the noise map
        for (int x = 0; x < _hNoiseSize.x; x++)
        {
            for (int y = 0; y < _hNoiseSize.y; y++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < _hNoiseOctavesOffset.Length; i += 2)
                {
                    float sampleX = (x - halfWidth + _hNoiseOctavesOffset[i]) / _hNoiseScale.x * frequency;
                    float sampleY = (y - halfHeight + _hNoiseOctavesOffset[i + 1]) / _hNoiseScale.y * frequency;

                    float value = noise.pnoise(new float2(sampleX, sampleY), new float2(sampleX, sampleY) * 2 - 1);

                    //float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += value * amplitude;

                    amplitude *= _hNoiseAmplitude;
                    frequency *= _hNoiseFrequency;
                }

                // Normalize the noiseHeight to be within the range of 0 to 1
                noiseHeight = (noiseHeight + maxPossibleNoiseValue) / (2 * maxPossibleNoiseValue);
                _hNoiseMap[x + y * _hNoiseSize.x] = (ushort)math.floor(noiseHeight * _hNoiseMagClamp);
            }
        }
    }

    void GenerateCaveNoise(ref Random random)
    {
        // create a max possible value for noise
        float maxPossibleNoiseValue = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < _cNoiseOctavesOffset.Length; i += 3)
        {
            _cNoiseOctavesOffset[i] = random.NextFloat(-100000, 100000) + _offset.x;
            _cNoiseOctavesOffset[i + 1] = random.NextFloat(-100000, 100000) + _offset.y;
            _cNoiseOctavesOffset[i + 2] = random.NextFloat(-100000, 100000) + _offset.z;

            maxPossibleNoiseValue += amplitude;
            amplitude *= _cNoiseAmplitude;
        }

        // loop through the noise map
        for (int x = 0; x < _cNoiseSize.x; x++)
        {
            for (int y = 0; y < _cNoiseSize.y; y++)
            {
                for (int z = 0; z < _cNoiseSize.z; z++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseValue = 0;

                    for (int i = 0; i < _cNoiseOctavesOffset.Length; i += 3)
                    {
                        float sampleX = (x + _cNoiseOctavesOffset[i]) / _cNoiseScale.x * frequency;
                        float sampleY = (y + _cNoiseOctavesOffset[i + 1]) / _cNoiseScale.y * frequency;
                        float sampleZ = (z + _cNoiseOctavesOffset[i + 2]) / _cNoiseScale.z * frequency;
                        float value = noise.snoise(new float3(sampleX, sampleY, sampleZ));
                        noiseValue += value * amplitude;
                        amplitude *= _cNoiseAmplitude;
                        frequency *= _cNoiseFrequency;
                    }

                    // Normalize the noiseValue to be within the range of 0 to 1
                    noiseValue = (noiseValue + maxPossibleNoiseValue) / (2 * maxPossibleNoiseValue);

                    // if the noiseValue is below the caveThreshold, set the voxel to air (0)
                    if (noiseValue < _cNoiseThreshold.x || noiseValue > _cNoiseThreshold.y)
                        _cNoiseMap[x + y * _cNoiseSize.x + z * _cNoiseSize.x * _cNoiseSize.y] = 0;
                    else
                        _cNoiseMap[x + y * _cNoiseSize.x + z * _cNoiseSize.x * _cNoiseSize.y] = 1;
                }
            }
        }
    }

    void AssignVoxels()
    {
        // loop through the x and z axis
        for (int x = 0; x < _chunkSize.x; x++)
        {
            for (int z = 0; z < _chunkSize.z; z++)
            {
                int height = _hNoiseMap[x + (z * _chunkSize.x)];
                // loop through the y axis
                for (int y = height; y >= 0; y--)
                {
                    int index3D = PosToIndex3D(x, y, z);
                    _voxels[index3D] = SetVoxel(new(x, y, z), height);
                }
            }
        }
    }

    uint SetVoxel(int3 pos, int surfaceHeight)
    {
        bool isVoxel = _cNoiseMap[PosToIndex3D(pos.x, pos.y, pos.z)] == 1;

        // BEDROCK
        if (pos.y == 0)
            return 2;

        // Top Soil
        if (surfaceHeight - pos.y < 3 && isVoxel)
            return 1;

        // Underground
        return (uint)(isVoxel ? 2 : 0);
    }

    int PosToIndex3D(int x, int y, int z) => x + y * _chunkSize.x + z * _chunkSize.x * _chunkSize.y;
}

[BurstCompile]
public struct VisibleVoxelsJob : IJob
{
    [ReadOnly] public NativeArray<uint> VoxelIds; // the IDs of each voxel in the chunk
    public NativeArray<int> result;
    public int3 chunkSize; // the size of the chunk in voxels

    public VisibleVoxelsJob(NativeArray<uint> voxelIds, NativeArray<int> resuts, int3 chunkSize)
    {
        VoxelIds = voxelIds;
        result = resuts;
        this.chunkSize = chunkSize;
    }

    public void Execute()
    {
        for (int i = 0; i < result.Length; i++)
        {
            // returns 0 if the voxel is air
            if (VoxelIds[i] == 0)
            {
                result[i] = 0;
                continue;
            }

            // Get the 3D position of the voxel
            int3 vPosInChunk = new int3(i % chunkSize.x, (i / chunkSize.x) % chunkSize.y, i / (chunkSize.x * chunkSize.y));
            int visibleSides = 0;
            for (int j = 0; j < WorldHelper.Directions.Length; j++)
                SetVisibility(ref visibleSides, vPosInChunk + WorldHelper.Directions[j], j);

            // Write the result to the output buffer
            result[i] = visibleSides;
        }
    }

    // sets a bit in an int to 1
    void SetBit(ref int num, int pos) => num = num | (1 << pos);

    // checks if a position is within the bounds of the chunk
    bool InBounds(int3 pos)
    {
        return pos.x >= 0 && pos.x < chunkSize.x &&
               pos.y >= 0 && pos.y < chunkSize.y &&
               pos.z >= 0 && pos.z < chunkSize.z;
    }

    void SetVisibility(ref int visInt, int3 voxelPos, int bitPos)
    {
        // if the voxel is in bounds, check if it is air, if it is, set the bit
        if (InBounds(voxelPos))
        {
            int index = voxelPos.x + voxelPos.y * chunkSize.x + voxelPos.z * chunkSize.x * chunkSize.y;
            if (VoxelIds[index] == 0)
                SetBit(ref visInt, bitPos);
        }

        // if the voxel is out of bounds, set the bit
        else
            SetBit(ref visInt, bitPos);
    }
}