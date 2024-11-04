using UnityEngine;
using Unity.Mathematics;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "World_Settings", menuName = EngineConstants.ENGINE_NAME + "/World_Settings", order = 1)]
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

        [SerializeField] int _seed = 0;
        public int Seed => _seed;

        [SerializeField] short _chunkTickRate = 1000;
        public short ChunkTickRate => _chunkTickRate;

        [SerializeField] NoiseProfile_SO _heightNoise;
        public NoiseProfile_SO HeightNoise => _heightNoise;

        [SerializeField] NoiseProfile_SO _caveNoise;
        public NoiseProfile_SO CaveNoise => _caveNoise;
    }
}
