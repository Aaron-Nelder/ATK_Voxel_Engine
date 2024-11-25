using UnityEngine;
using Unity.Mathematics;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Noise Profile", menuName = EngineConstants.ENGINE_NAME + "/Noise Profile")]
    public class NoiseProfile_SO : ScriptableObject
    {
        [SerializeField] NoiseDimension _dimension = NoiseDimension.TWO_D;
        public NoiseDimension Dimension => _dimension;

        [SerializeField] NoiseType _type = NoiseType.PERLIN;
        public NoiseType Type => _type;

        [SerializeField] float3 _scale = new(1, 1, 1);
        public float3 Scale => _scale;

        [SerializeField] float _amplitude = 1.0f;
        public float Amplitude => _amplitude;

        [SerializeField] float _frequency = 0.01f;
        public float Frequency => _frequency;

        [SerializeField] int _octaves = 4;
        public int Octaves => _octaves;

        [SerializeField] float _lacunarity = 2.0f;
        public float Lacunarity => _lacunarity;

        [SerializeField] float _persistence = 0.5f;
        public float Persistence => _persistence;

        [SerializeField] float _rotation = 0;
        public float Rotation => _rotation;

        [SerializeField] uint _multiplier = 1;
        public uint Multiplier => _multiplier;

        [SerializeField] bool _useThreshold = false;
        public bool UseThreshold => _useThreshold;

        [SerializeField] float2 _threshold = new float2(0.45f, 1);
        public float2 Threshold => _threshold;
    }
}