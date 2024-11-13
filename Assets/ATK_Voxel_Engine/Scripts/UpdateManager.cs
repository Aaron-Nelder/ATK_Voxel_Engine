using System;
using UnityEngine;
using System.Threading.Tasks;

namespace ATKVoxelEngine
{
    public enum UpdateType { UPDATE, FIXED_UPDATE, LATE_UPDATE }

    public interface IUpdatable
    {
        public abstract UpdateType UpdateType { get; }
        public abstract void Register();
        public abstract void UnRegister();
        public abstract void Tick(float deltaTime);
    }

    public class UpdateManager : MonoBehaviour
    {
        public static event Action<float> OnUpdate;
        public static event Action<float> OnFixedUpdate;
        public static event Action<float> OnLateUpdate;
        public static event Action<float> OnEngineTick;

        async void Awake()
        {
            DontDestroyOnLoad(this);
            await TickEngine();
        }

        public async Task TickEngine()
        {
            float _lastEngineTickTime = Time.time;

            while (true)
            {
                await Task.Delay(EngineSettings.TickRateSettings.EngineTickRate);
                OnEngineTick?.Invoke(Time.time - _lastEngineTickTime);
            }
        }

        #region Unity Update Functions
        void Update() => OnUpdate?.Invoke(Time.deltaTime);
        void FixedUpdate() => OnFixedUpdate?.Invoke(Time.fixedDeltaTime);
        void LateUpdate() => OnLateUpdate?.Invoke(Time.deltaTime);
        #endregion
    }
}
