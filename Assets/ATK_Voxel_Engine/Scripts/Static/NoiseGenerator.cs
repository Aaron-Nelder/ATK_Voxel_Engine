using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace ATKVoxelEngine
{
    [BurstCompile]
    public struct NoiseJob : IJob
    {
        int3 _size, _offset;
        float _persistence, _lacunarity, _amplitude, _frequency, _rotation;
        uint _seed, _multiplier;
        float3 _scale;

        float2 _threshold;

        NoiseType _noiseType;
        NoiseDimension _noiseDimension;

        NativeArray<int> _output;
        NativeArray<float> _octaves;

        // called when generationg a chunk
        public NoiseJob(WorldSettings_SO wSettings, NoiseProfile_SO noiseSettings, int3 offset, NativeArray<int> noise, NativeArray<float> octaves)
        {
            _size = wSettings.ChunkSize;
            _scale = noiseSettings.Scale.Equals(new(0, 0, 0)) ? new float3(1, 1, 1) : noiseSettings.Scale;
            _seed = wSettings.Seed != default ? wSettings.Seed : 69;
            _multiplier = noiseSettings.Multiplier != 0 ? noiseSettings.Multiplier : 1;
            _persistence = noiseSettings.Persistence;
            _lacunarity = noiseSettings.Lacunarity;
            _offset = offset;
            _amplitude = noiseSettings.Amplitude;
            _frequency = noiseSettings.Frequency;
            _rotation = noiseSettings.Rotation;

            if (noiseSettings.UseThreshold)
                _threshold = noiseSettings.Threshold;
            else
                _threshold = new float2(0, 0);

            _noiseType = noiseSettings.Type;
            _noiseDimension = noiseSettings.Dimension;

            _output = noise;
            _octaves = octaves;
        }

        // called from editor for preview texture
        public NoiseJob(NoiseProfile_SO noiseSettings, NativeArray<int> noise, NativeArray<float> octaves, uint seed, int3 size, int3 offset)
        {
            _size = size;
            _scale = noiseSettings.Scale.Equals(new(0, 0, 0)) ? new float3(1, 1, 1) : noiseSettings.Scale;
            _seed = seed;
            _multiplier = noiseSettings.Multiplier != 0 ? noiseSettings.Multiplier : 1;
            _persistence = noiseSettings.Persistence;
            _lacunarity = noiseSettings.Lacunarity;
            _offset = offset;
            _amplitude = noiseSettings.Amplitude;
            _frequency = noiseSettings.Frequency;
            _rotation = noiseSettings.Rotation;

            _threshold = new float2(0, 0);

            _noiseType = noiseSettings.Type;
            _noiseDimension = NoiseDimension.TWO_D;

            _output = noise;
            _octaves = octaves;
        }

        public void Execute()
        {
            Random random = new Random(_seed);

            switch (_noiseDimension)
            {
                case NoiseDimension.TWO_D:
                    Proc2DNoise(ref random);
                    break;
                case NoiseDimension.THREE_D:
                    Proc3DNoise(ref random);
                    break;
            }

        }

        void Proc2DNoise(ref Random random)
        {
            float maxAmplitude = 0;
            float amplitude = 1;
            float frequency = _frequency;

            // Generates the octaves
            for (int i = 0; i < _octaves.Length; i += 2)
            {
                _octaves[i] = random.NextFloat(-100000, 100000) + _offset.x;
                _octaves[i + 1] = random.NextFloat(-100000, 100000) + _offset.z;
                maxAmplitude += amplitude;
                amplitude *= _amplitude;
            }

            float halfWidth = _size.x / 2.0f;
            float halfDepth = _size.z / 2.0f;

            // loop through the noise map
            for (int x = 0; x < _size.x; x++)
            {
                for (int z = 0; z < _size.z; z++)
                {
                    float noiseValue = 0;
                    amplitude = 1;
                    frequency = _frequency;

                    for (int i = 0; i < _octaves.Length; i += 2)
                    {
                        // Calculate the sample position
                        float2 samplePosition;
                        samplePosition.x = (x - halfWidth + _octaves[i]) / _scale.x * frequency;
                        samplePosition.y = (z - halfDepth + _octaves[i + 1]) / _scale.z * frequency;

                        // Get the noise value at the sample position
                        float value = 0;
                        switch (_noiseType)
                        {
                            case NoiseType.CLASSIC:
                                value = noise.cnoise(samplePosition * _lacunarity * 2 - 1);
                                break;
                            case NoiseType.PERLIN:
                                value = noise.pnoise(samplePosition * _lacunarity, new float2(_size.x, _scale.z) * 2 - 1);
                                break;
                            case NoiseType.PERLIN_ROTATION:
                                value = noise.psrnoise(samplePosition * _lacunarity, new float2(_size.x, _scale.z) * 2 - 1, _rotation);
                                break;
                            case NoiseType.SIMPLEX:
                                value = noise.snoise(samplePosition * _lacunarity * 2 - 1);
                                break;
                            case NoiseType.SIMPLEX_ROTATION:
                                value = noise.srnoise(samplePosition * _lacunarity * 2 - 1, _rotation);
                                break;
                            case NoiseType.CELLUAR:
                                float2 cellValue = noise.cellular(samplePosition * _lacunarity * 2 - 1);
                                value = cellValue.x;
                                break;
                        }

                        noiseValue += value * amplitude;

                        amplitude *= _persistence;
                        frequency *= _lacunarity;
                    }

                    noiseValue = (noiseValue + maxAmplitude) / (2 * maxAmplitude);  // Normalizes the noiseValue to be within the range of 0 to 1
                    int intValue = (int)math.floor(noiseValue * _multiplier);       // Rounds the noiseValue to the nearest whole number between 0 and _magClamp

                    if (!_threshold.Equals(new float2(0, 0)))
                    {
                        if (intValue < _threshold.x || intValue > _threshold.y)
                            intValue = 0;
                        else
                            intValue = 1;
                    }

                    intValue = (int)math.clamp(intValue, 0, _multiplier);
                    _output[x + z * _size.x] = intValue;
                }
            }
        }

        void Proc3DNoise(ref Random random)
        {
            float maxAmplitude = 0;
            float amplitude = 1;
            float frequency = _frequency;

            // Generates the octaves
            for (int i = 0; i < _octaves.Length; i += 3)
            {
                _octaves[i] = random.NextFloat(-100000, 100000) + _offset.x;
                _octaves[i + 1] = random.NextFloat(-100000, 100000) + _offset.y;
                _octaves[i + 2] = random.NextFloat(-100000, 100000) + _offset.z;

                maxAmplitude += amplitude;
                amplitude *= _amplitude;
            }

            float halfWidth = _size.x / 2.0f;
            float halfHeight = _size.y / 2.0f;
            float halfDepth = _size.z / 2.0f;

            // loop through the noise map
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    for (int z = 0; z < _size.z; z++)
                    {
                        float noiseValue = 1;
                        amplitude = 1;
                        frequency = _frequency;

                        for (int i = 0; i < _octaves.Length; i += 3)
                        {
                            // Calculate the sample position
                            float3 samplePosition;
                            samplePosition.x = (x - halfWidth + _octaves[i]) / _scale.x * frequency;
                            samplePosition.y = (y - halfHeight + _octaves[i + 1]) / _scale.y * frequency;
                            samplePosition.z = (z - halfDepth + _octaves[i + 2]) / _scale.z * frequency;

                            // Get the noise value at the sample position
                            float value = 1;
                            float xy = 0;
                            float xz = 0;
                            float yz = 0;
                            switch (_noiseType)
                            {
                                case NoiseType.CLASSIC:
                                    value = noise.cnoise(samplePosition * _lacunarity * 2 - 1);
                                    break;
                                case NoiseType.PERLIN:
                                    value = noise.pnoise(samplePosition * _lacunarity, _scale * 2 - 1);
                                    break;
                                case NoiseType.PERLIN_ROTATION:
                                    xy = noise.psrnoise(new float2(samplePosition.x, samplePosition.y) * _lacunarity, _scale.xy * 2 - 1, _rotation);
                                    xz = noise.psrnoise(new float2(samplePosition.x, samplePosition.z) * _lacunarity, _scale.xz * 2 - 1, _rotation);
                                    yz = noise.psrnoise(new float2(samplePosition.y, samplePosition.z) * _lacunarity, _scale.yz * 2 - 1, _rotation);
                                    value = noise.pnoise(new float3(xy, xz, yz), _scale * 2 - 1) / 3;
                                    break;
                                case NoiseType.SIMPLEX:
                                    value = noise.snoise((samplePosition * _lacunarity) * 2 - 1);
                                    break;
                                case NoiseType.SIMPLEX_ROTATION:
                                    xy = noise.srnoise(new float2(samplePosition.x, samplePosition.y) * _lacunarity * 2 - 1, _rotation);
                                    xz = noise.srnoise(new float2(samplePosition.x, samplePosition.z) * _lacunarity * 2 - 1, _rotation);
                                    yz = noise.srnoise(new float2(samplePosition.y, samplePosition.z) * _lacunarity * 2 - 1, _rotation);
                                    value = noise.snoise(new float3(xy, xz, yz) * 2 - 1) / 3;
                                    break;
                                case NoiseType.CELLUAR:
                                    value = noise.cellular(samplePosition * _lacunarity * 2 - 1).x;
                                    break;
                            }

                            noiseValue += value * amplitude;
                            amplitude *= _persistence;
                            frequency *= _lacunarity;
                        }

                        noiseValue = (noiseValue + maxAmplitude) / (2 * maxAmplitude);  // Normalizes the noiseValue to be within the range of 0 to 1
                        int intValue = (int)math.floor(noiseValue * _multiplier);       // Rounds the noiseValue to the nearest whole number between 0 and _magClamp

                        if (!_threshold.Equals(new float2(0, 0)))
                            intValue = intValue > _threshold.x && intValue < _threshold.y ? 0 : 1;

                        _output[x + y * _size.x + z * _size.x * _size.y] = intValue;
                    }
                }
            }
        }
    }
}