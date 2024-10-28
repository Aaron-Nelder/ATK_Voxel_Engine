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
            ChunkManager.SpawnStartingChunks(transform, false);
            new Selector();
        }

        public static void OnCenterChunkInit()
        {
            SetPlayerSpawnPoint();
        }

        static void SetPlayerSpawnPoint()
        {
            if (PlayerManager.Instance == null) return;

            Vector3 newPos = new Vector3(EngineSettings.WorldSettings.ChunkSize.x / 2, 0, EngineSettings.WorldSettings.ChunkSize.z / 2);
            PlayerManager.Instance.transform.position = newPos;
            PlayerHelper.SnapPlayerToSurface();
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