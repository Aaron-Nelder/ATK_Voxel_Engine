using UnityEngine;
using System;
using Unity.Profiling;

public static class DebugHelper
{
    static bool debugging = true;
    public static bool Debugging
    {
        get => debugging;
        set
        {
            debugging = value;
            OnDebugging?.Invoke(value);
        }
    }

    public const string MENU_NAME = "ATK Voxel Engine";

    public static Action<bool> OnDebugging;

    static int lastframes = 0;
    static float lastTime = 0.0f;
    public static string FPS
    {
        get
        {
            int frameDif = Time.frameCount - lastframes;
            lastframes = Time.frameCount;

            float timeDif = Time.unscaledTime - lastTime;

            float fpsCounter = frameDif / timeDif;
            lastTime = Time.unscaledTime;
            return string.Format("{0:0.} FPS", fpsCounter);
        }
    }

    static bool capturingBatches = false;
    static ProfilerRecorder batchesRecorder;

    static ChunkPosition lastKnowCameraChunk = new();
    static Vector3Int lastKnowCamPos;
    public static ChunkPosition CameraChunk
    {
        get
        {
            if (Camera.current is null)
                return lastKnowCameraChunk;

            Vector3 cameraPos = Camera.current.transform.position;
            int chunkSize = VoxelManager.WorldSettings.chunkSize;
            lastKnowCameraChunk = new ChunkPosition(Mathf.FloorToInt(cameraPos.x / chunkSize), Mathf.FloorToInt(cameraPos.z / chunkSize));
            return lastKnowCameraChunk;
        }
    }

    public static Vector3Int CameraPos
    {
        get
        {
            if (Camera.current is null)
                return lastKnowCamPos;
            return Vector3Int.FloorToInt(Camera.current.transform.position);
        }
    }

    public static float CPUTime
    {
        get
        {
            FrameTiming[] frameTimings = new FrameTiming[1];
            FrameTimingManager.CaptureFrameTimings();
            uint frameCount = FrameTimingManager.GetLatestTimings(1, frameTimings);

            return frameCount > 0 ? MathF.Round((float)frameTimings[0].cpuFrameTime, 2) : 0;
        }
    }

    public static float GPUTime
    {
        get
        {
            FrameTiming[] frameTimings = new FrameTiming[1];
            FrameTimingManager.CaptureFrameTimings();
            uint frameCount = FrameTimingManager.GetLatestTimings(1, frameTimings);

            return frameCount > 0 ? MathF.Round((float)frameTimings[0].gpuFrameTime, 2) : 0;
        }
    }

    public static long Batches
    {
        get
        {
            long val = -1;
#if UNITY_EDITORs
            val = UnityEditor.UnityStats.batches;
#endif
#if UNITY_STANDALONE

            if (!capturingBatches)
            {
                batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", 1);
                capturingBatches = true;
            }

            val = batchesRecorder.LastValue;
#endif
            return val;
        }
    }

    static void Dispose()
    {
        batchesRecorder.Dispose();
    }
}

