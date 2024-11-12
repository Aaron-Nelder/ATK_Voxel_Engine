using UnityEngine;
using Unity.Mathematics;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Noise Profile", menuName = EngineConstants.ENGINE_NAME + "/Noise Profile")]
    public class NoiseProfile_SO : ScriptableObject
    {
        [SerializeField] float _scale = 200.0f;
        public float Scale => _scale;

        [SerializeField] float _amplitude = 1.0f;
        public float Amplitude => _amplitude;

        [SerializeField] float _frequency = 0.01f;
        public float Frequency => _frequency;

        [SerializeField] int _octaves = 4;
        public int Octaves => _octaves;

        [SerializeField] float _lacunarity = 2.0f;
        public float Lacunarity => _lacunarity;

        [SerializeField] float _persistance = 0.5f;
        public float Persistance => _persistance;

        [SerializeField] int _magClamp = 1;
        public int MagClamp => _magClamp;

        [SerializeField] bool _useThreshold = false;
        public bool UseThreshold => _useThreshold;

        [SerializeField] float2 _threshold = new float2(0.45f, 1);
        public float2 Threshold => _threshold;
    }
}