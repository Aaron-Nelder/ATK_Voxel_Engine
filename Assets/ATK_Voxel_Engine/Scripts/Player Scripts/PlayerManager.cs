using UnityEngine;
using ATKVoxelEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    static ChunkPosition playerChunk = new(0, 0);

    [field: SerializeField] public EntityStats_SO Stats { get; private set; }
    [field: SerializeField] public EntityMotionHandler MotionHandler { get; private set; }
    [field: SerializeField] public PlayerCamera PlayerCamera { get; private set; }
    [field: SerializeField] public RightHand RightHand { get; private set; }

    void Awake()
    {
        Instance = this;
        MotionHandler.Init(Stats);
    }

    void FixedUpdate()
    {
        ChunkPosition currentChunk = PlayerHelper.PlayerChunk;
        if (playerChunk != currentChunk)
        {
            ChunkManager.IvokeOnPlayerChunkUpdate(currentChunk, currentChunk - playerChunk);
            playerChunk = currentChunk;
        }
    }
}
