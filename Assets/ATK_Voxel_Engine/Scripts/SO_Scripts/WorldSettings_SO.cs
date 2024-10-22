using System;
using UnityEngine;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "World_Settings", menuName = EngineConstants.ENGINE_NAME + "/World_Settings", order = 1)]
    public class WorldSettings_SO : ScriptableObject
    {
        [SerializeField] GameObject _chunkPrefab;
        public GameObject ChunkPrefab => _chunkPrefab;

        [SerializeField] Vector3Int _chunkSize = new Vector3Int(16, 128, 16);
        public Vector3Int ChunkSize => _chunkSize;

        [SerializeField] UInt16 _renderDistance = 4;
        public UInt16 RenderDistance => _renderDistance;

        [SerializeField] int _seed = 0;
        public int Seed => _seed;

        [SerializeField] Int16 _chunkTickRate = 1000;
        public Int16 ChunkTickRate => _chunkTickRate;

        [SerializeField] NoiseProfile_SO _heightNoise;
        public NoiseProfile_SO HeightNoise => _heightNoise;

        [SerializeField] NoiseProfile_SO _caveNoise;
        public NoiseProfile_SO CaveNoise => _caveNoise;
    }
}
