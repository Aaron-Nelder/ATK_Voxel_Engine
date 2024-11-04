using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ATKVoxelEngine
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Inspector
        [field: SerializeField] public PoolManager Pool { get; private set; }

        public static Action OnChunkTick;      

        static float _chunkTick = 0;
        static Int16 _cachedTickRate = 0;

        void Awake()
        {
            Instance = this;

            Pool.SetupPools();

            if (!EngineSettings.GatherSO())
                return;

            _cachedTickRate = EngineSettings.WorldSettings.ChunkTickRate;
            ChunkManager.SpawnStartingChunks(transform);
            new Selector();
        }

        public static void OnCenterChunkInit()
        {
            SetPlayerSpawnPoint();
        }

        static void SetPlayerSpawnPoint()
        {
            if (PlayerManager.Instance == null) return;

            PlayerHelper.SnapPlayerToVoxel(ChunkManager.Chunks[new(0,0)], EngineSettings.WorldSettings.ChunkSize.x / 2, EngineSettings.WorldSettings.ChunkSize.z / 2);
        }

        void FixedUpdate()
        {
            ChunkTick();
        }

        void ChunkTick()
        {
            _chunkTick += Time.deltaTime;

            if (_chunkTick * 1000 >= _cachedTickRate)
            {
                OnChunkTick?.Invoke();
                _chunkTick = 0;
            }
        }
    }
}