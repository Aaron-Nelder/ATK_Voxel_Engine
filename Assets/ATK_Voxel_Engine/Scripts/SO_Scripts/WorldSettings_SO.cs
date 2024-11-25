using UnityEngine;
using Unity.Mathematics;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "World_Settings", menuName = EngineConstants.ENGINE_NAME + "/World Settings", order = 1)]
    public class WorldSettings_SO : ScriptableObject
    {
        [SerializeField] GameObject _chunkPrefab;
        public GameObject ChunkPrefab => _chunkPrefab;

        [SerializeField] int3 _chunkSize = new int3(16, 128, 16);
        public int3 ChunkSize => _chunkSize;

        [SerializeField] Bounds _chunkBounds = new Bounds(Vector3.zero, new Vector3(16, 128, 16));
        public Bounds ChunkBounds => _chunkBounds;

        [SerializeField] ushort _renderDistance = 4;
        public ushort RenderDistance => _renderDistance;

        [SerializeField] uint _seed = 0;
        public uint Seed => _seed;

        [SerializeField] float2 _humidityRange = new float2(0, 1);
        public float2 HumidityRange => _humidityRange;

        [SerializeField] float2 _temperatureRange = new float2(-45.0f, 45.0f);
        public float2 TemperatureRange => _temperatureRange;

        [SerializeField] NoiseProfile_SO _humidityNoise;
        public NoiseProfile_SO HumidityNoise => _humidityNoise;

        [SerializeField] NoiseProfile_SO _temperatureNoise;
        public NoiseProfile_SO TemperatureNoise => _temperatureNoise;
    }
}
