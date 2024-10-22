using UnityEngine;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Debug Settings", menuName = EngineConstants.ENGINE_NAME + "/Debug Settings")]
    public class DebugSettings_SO : ScriptableObject
    {
        [Header("Editor View Labels")]
        public bool showEditorInGameMode = true;
        public bool editorShowFPS = true;
        public bool editorShowActiveChunks = true;
        public bool editorShowPlayerChunk = true;
        public bool editorShowPlayerPos = true;
        public bool editorShowCameraChunk = true;
        public bool editorShowCameraPos = true;
        public bool editorShowCPUTime = true;
        public bool editorShowGPUTime = true;
        public bool editorShowBatches = true;

        [Header("Game View Labels")]
        public bool gameShowFPS = true;
        public bool gameShowPlayerChunk = true;
        public bool gameShowPlayerPos = true;
        public bool gameShowActiveChunks = true;
        public bool gameShowActiveVoxels = true;
        public bool gameShowCPUTime = true;
        public bool gameShowGPUTime = true;
        public bool gameShowBatches = true;

        public DebugStyle camChunkBorder;
        public DebugStyle playerChunkBorder;

        public GameObject debugLinePrefab;
    }

    [System.Serializable]
    public struct DebugStyle
    {
        public bool enabled;
        public PreviewType previewType;
        public Color color;
    }

    public enum PreviewType
    {
        WireCube,
        WireSphere,
        Line,
        DashLine,
    }
}