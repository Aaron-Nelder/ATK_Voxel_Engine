using UnityEngine;
using ATKVoxelEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [field: SerializeField] public EntityStats_SO Stats { get; private set; }
    [field: SerializeField] public EntityMotionHandler MotionHandler { get; private set; }
    [field: SerializeField] public Camera PlayerCamera { get; private set; }
    [field: SerializeField] public RightHand RightHand { get; private set; }
    [field: SerializeField] public HudManager HUD { get; private set; }

    void Awake()
    {
        Instance = this;
        EngineManager.OnGameStateChange += OnGameStateChanged;
    }

    void OnGameStateChanged(GameState previousGameState, GameState newGameState)
    {
        switch (newGameState)
        {
            case GameState.LOADING:
                MotionHandler.Rigidbody.isKinematic = true;
                MotionHandler.Rigidbody.useGravity = false;
                break;
            case GameState.PLAYING:
                MotionHandler.Rigidbody.isKinematic = false;
                MotionHandler.Rigidbody.useGravity = true;
                PlayerHelper.SnapPlayerToVoxel(ChunkManager.Chunks[new(0, 0)], EngineSettings.WorldSettings.ChunkSize.x / 2, EngineSettings.WorldSettings.ChunkSize.z / 2);
                MotionHandler.Init(Stats);
                HUD.Initialize();
                Selector.Register();
                break;
            case GameState.EDITOR:
                break;
            case GameState.MAIN_MENU:
                break;
        }
    }

    void OnDestroy() => EngineManager.OnGameStateChange -= OnGameStateChanged;
}
