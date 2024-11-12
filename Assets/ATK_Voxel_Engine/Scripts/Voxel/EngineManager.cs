using System;
using UnityEngine;

namespace ATKVoxelEngine
{
    public enum GameState { NULL, LOADING, PLAYING, EDITOR, MAIN_MENU }

    public class EngineManager : MonoBehaviour
    {
        public static EngineManager Instance { get; private set; }

        public static GameState CurrentGameState { get; private set; } = GameState.NULL;
        public static event Action<GameState, GameState> OnGameStateChange;

        // Inspector
        [field: SerializeField] public PoolManager Pool { get; private set; }

        void Awake()
        {
            Instance = this;

            SetGameState(GameState.LOADING);

            Pool.SetupPools();

            if (!EngineSettings.GatherSO())
                return;

            ChunkManager.SpawnStartingChunks(transform);
            Selector.Register();
        }

        public static void SetGameState(GameState state)
        {
            if (CurrentGameState == state) return;

            GameState previousGameState = CurrentGameState;
            CurrentGameState = state;

            OnGameStateChange?.Invoke(previousGameState, CurrentGameState);
        }

        void OnDestroy() => Selector.UnRegister();
    }
}