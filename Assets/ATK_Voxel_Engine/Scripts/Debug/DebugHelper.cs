using UnityEngine;
using System;
using Unity.Profiling;

namespace ATKVoxelEngine
{
    public static class DebugHelper
    {
        static bool debugging = false;
        public static bool Debugging
        {
            get => debugging;
            set
            {
                debugging = value;
                OnDebugging?.Invoke(value);
            }
        }

        public static Action<bool> OnDebugging;

        static float lastGetTime = 0;
        static int lastFrameCount = 0;
        public static string FPS
        {
            get
            {
                int frameCount = Time.frameCount - lastFrameCount;
                float time = Time.time - lastGetTime;
                int fps = (int)(frameCount / time);

                lastGetTime = Time.time;
                lastFrameCount = Time.frameCount;
                return $"FPS: {MathF.Round(fps).ToString()}";
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
                lastKnowCameraChunk = new ChunkPosition(Mathf.FloorToInt(cameraPos.x / EngineSettings.WorldSettings.ChunkSize.x), Mathf.FloorToInt(cameraPos.z / EngineSettings.WorldSettings.ChunkSize.z));
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
}
