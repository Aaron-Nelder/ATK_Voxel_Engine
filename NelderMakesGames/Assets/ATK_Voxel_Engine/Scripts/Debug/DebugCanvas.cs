using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DebugCanvas : MonoBehaviour
{
    [SerializeField] GameObject canvas;

    [SerializeField] int tickRate = 10;
    int tick;

    [Header("Texts")]
    public TMP_Text fps;
    public TMP_Text playerChunk;
    public TMP_Text playerPosition;
    public TMP_Text activeChunks;
    public TMP_Text activeVoxels;
    public TMP_Text CPUTime;
    public TMP_Text GPUTime;
    public TMP_Text Batches;

    void OnEnable()
    {
        DebugHelper.Debugging = canvas.gameObject.activeSelf;
        DebugHelper.OnDebugging += OnDebuggingChanged;
    }

    void OnDebuggingChanged(bool value)
    {
        tick = tickRate;
        canvas.SetActive(value);
    }

    void FixedUpdate()
    {
        if (!canvas.gameObject.activeSelf) return;

        tick++;

        if (tick < tickRate) return;

        if (VoxelManager.DebugSettings.gameShowPlayerChunk)
            playerChunk.text = $"Chunk: {PlayerHelper.PlayerChunk.x}, {PlayerHelper.PlayerChunk.z}";

        if (VoxelManager.DebugSettings.gameShowPlayerPos)
            playerPosition.text = $"Position: {PlayerHelper.PlayerVoxelPosition.x},{PlayerHelper.PlayerVoxelPosition.y}, {PlayerHelper.PlayerVoxelPosition.z}";

        if (VoxelManager.DebugSettings.gameShowFPS)
            fps.text = DebugHelper.FPS;

        if (VoxelManager.DebugSettings.gameShowActiveChunks)
            activeChunks.text = $"Active Chunks: {ChunkManager.Chunks.Count}";

        if (VoxelManager.DebugSettings.gameShowActiveVoxels)
            activeVoxels.text = $"Active Voxels: {ChunkManager.Chunks.Count * VoxelManager.WorldSettings.chunkSize * VoxelManager.WorldSettings.chunkSize * (VoxelManager.WorldSettings.worldHeight * 2)}";

        if (VoxelManager.DebugSettings.gameShowCPUTime)
            CPUTime.text = $"CPU Time: {DebugHelper.CPUTime} ms";

        if (VoxelManager.DebugSettings.gameShowGPUTime)
            GPUTime.text = $"GPU Time: {DebugHelper.GPUTime} ms";

        if (VoxelManager.DebugSettings.gameShowBatches)
            Batches.text = $"Batches: {DebugHelper.Batches}";

        tick = 0;
    }

    void OnDisable()
    {
        DebugHelper.OnDebugging -= OnDebuggingChanged;
    }
}
