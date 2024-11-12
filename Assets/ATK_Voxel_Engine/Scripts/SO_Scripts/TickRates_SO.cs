using UnityEngine;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Tick Rates", menuName = EngineConstants.ENGINE_NAME + "/Tick Rates")]
    public class TickRates_SO : ScriptableObject
    {
        [SerializeField] short _engineTickRate = 1000;
        public short EngineTickRate => _engineTickRate;

        [SerializeField] short _chunkTickRate = 1000;
        public short ChunkTickRate => _chunkTickRate;

        [SerializeField] short _playerTickRate = 1000;
        public short PlayerTickRate => _playerTickRate;

        [SerializeField] short _entityTickRate = 1000;
        public short EntityTickRate => _entityTickRate;

        [SerializeField] short _chunkLoadTickRate = 1000;
        public short ChunkLoadTickRate => _chunkLoadTickRate;
    }
}
