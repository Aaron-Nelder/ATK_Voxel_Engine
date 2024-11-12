#pragma warning disable CS4014

using System;
using UnityEngine;
using System.Threading.Tasks;

namespace ATKVoxelEngine
{
    public enum TickType { UPDATE, FIXED_UPDATE, LATE_UPDATE, ENGINE, CHUNK, PLAYER, ENTITY, CHUNK_LOAD }

    public interface ITickable
    {
        public abstract TickType TickType { get; }
        public abstract void Register();
        public abstract void UnRegister();
        public abstract void Tick(float deltaTime);
    }

    public class TickRateManager : MonoBehaviour
    {
        // Unity Events
        public static event Action<float> OnUpdate;
        public static event Action<float> OnFixedUpdate;
        public static event Action<float> OnLateUpdate;

        // Engine Events
        public static event Action<float> OnEngineTick;
        public static event Action<float> OnChunkTick;
        public static event Action<float> OnPlayerTick;
        public static event Action<float> OnEntityTick;
        public static event Action<float> OnChunkLoadTick;

        void Awake()
        {
            DontDestroyOnLoad(this);

            TickEngine();
            TickChunk();
            TickPlayer();
            TickEntity();
            TickChunkLoad();
        }

        async Task TickEngine()
        {
            float _lastEngineTickTime = Time.time;
            while (true)
            {
                if (destroyCancellationToken.IsCancellationRequested) return;
                await Task.Delay(EngineSettings.TickRateSettings.EngineTickRate);
                OnEngineTick?.Invoke(Time.time - _lastEngineTickTime);
            }
        }

        async Task TickChunk()
        {
            float _lastChunkTickTime = Time.time;
            while (true)
            {
                if (destroyCancellationToken.IsCancellationRequested) return;
                await Task.Delay(EngineSettings.TickRateSettings.ChunkTickRate);
                OnChunkTick?.Invoke(Time.time - _lastChunkTickTime);
            }
        }

        async Task TickPlayer()
        {
            float _lastPlayerTickTime = Time.time;
            while (true)
            {
                if (destroyCancellationToken.IsCancellationRequested) return;
                await Task.Delay(EngineSettings.TickRateSettings.PlayerTickRate);
                OnPlayerTick?.Invoke(Time.time - _lastPlayerTickTime);
            }
        }

        async Task TickEntity()
        {
            float _lastEntityTickTime = Time.time;
            while (true)
            {
                if (destroyCancellationToken.IsCancellationRequested) return;
                await Task.Delay(EngineSettings.TickRateSettings.EntityTickRate);
                OnEntityTick?.Invoke(Time.time - _lastEntityTickTime);
            }
        }

        async Task TickChunkLoad()
        {
            float _lastChunkLoadTickTime = Time.time;
            while (true)
            {
                if (destroyCancellationToken.IsCancellationRequested) return;
                await Task.Delay(EngineSettings.TickRateSettings.ChunkLoadTickRate);
                OnChunkLoadTick?.Invoke(Time.time - _lastChunkLoadTickTime);
            }
        }

        #region Unity Update Functions
        void Update() => OnUpdate?.Invoke(Time.deltaTime);
        void FixedUpdate() => OnFixedUpdate?.Invoke(Time.deltaTime);
        void LateUpdate() => OnLateUpdate?.Invoke(Time.deltaTime);
        #endregion
    }
}
