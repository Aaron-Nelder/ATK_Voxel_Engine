using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    public class DebugHud : ITickable
    {
        public TickType TickType => TickType.FIXED_UPDATE;
        public bool Registered { get; private set; }

        Label _fps;
        Label _playerChunk;
        Label _playerPosition;
        Label _activeChunks;
        Label _activeVoxels;
        Label _activeVoxelsPosition;
        Label _CPUTime;
        Label _GPUTime;
        Label _Batches;

        public DebugHud(UIDocument hudDocument)
        {
            _fps = hudDocument.rootVisualElement.Q<Label>("FPS");
            _playerChunk = hudDocument.rootVisualElement.Q<Label>("PlayerChunk");
            _playerPosition = hudDocument.rootVisualElement.Q<Label>("PlayerPosition");
            _activeChunks = hudDocument.rootVisualElement.Q<Label>("ActiveChunks");
            _activeVoxels = hudDocument.rootVisualElement.Q<Label>("ActiveVoxels");
            _activeVoxelsPosition = hudDocument.rootVisualElement.Q<Label>("ActiveVoxelsPosition");
            _CPUTime = hudDocument.rootVisualElement.Q<Label>("CPUTime");
            _GPUTime = hudDocument.rootVisualElement.Q<Label>("GPUTime");
            _Batches = hudDocument.rootVisualElement.Q<Label>("Batches");
            Register();
        }

        public void Register()
        {
            TickRateManager.OnFixedUpdate += Tick;
            Registered = true;
        }

        public void UnRegister()
        {
            TickRateManager.OnFixedUpdate -= Tick;
            Registered = false;
        }

        public void Tick(float deltaTime)
        {
            if (EngineSettings.DebugSettings.gameShowFPS)
                _fps.text = DebugHelper.FPS;

            if (EngineSettings.DebugSettings.gameShowPlayerChunk)
                _playerChunk.text = $"Chunk: {PlayerHelper.PlayerChunk.x}, {PlayerHelper.PlayerChunk.z}";

            if (EngineSettings.DebugSettings.gameShowPlayerPos)
                _playerPosition.text = $"Position: {PlayerHelper.PlayerVoxelPosition.x},{PlayerHelper.PlayerVoxelPosition.y}, {PlayerHelper.PlayerVoxelPosition.z}";

            if (EngineSettings.DebugSettings.gameShowActiveChunks)
                _activeChunks.text = $"Active Chunks: {ChunkManager.Chunks.Count}";

            if (EngineSettings.DebugSettings.gameShowActiveVoxels)
                _activeVoxels.text = $"Active Voxels: {ChunkManager.Chunks.Count * EngineSettings.WorldSettings.ChunkSize}";

            if (EngineSettings.DebugSettings.gameShowCPUTime)
                _CPUTime.text = $"CPU Time: {DebugHelper.CPUTime} ms";

            if (EngineSettings.DebugSettings.gameShowGPUTime)
                _GPUTime.text = $"GPU Time: {DebugHelper.GPUTime} ms";

            if (EngineSettings.DebugSettings.gameShowBatches)
                _Batches.text = $"Batches: {DebugHelper.Batches}";
        }
    }
}