using UnityEngine;
using System.Threading.Tasks;
using System;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace ATKVoxelEngine
{
    [RequireComponent(typeof(ChunkRenderManager))]
    public class Chunk : MonoBehaviour
    {
        public ChunkPosition Position { get; private set; }
        public int3 WorldPosition { get; private set; }
        public bool Initialized { get; private set; }
        public int3 ChunkSize { get; private set; }

        [field: SerializeField] public ChunkRenderManager RenderManager { get; private set; }

        #region Voxel Get/Set
        //uint[] _voxels = new uint[0];
        NativeArray<uint> _voxels;
        public void SetVoxel(int3 index, uint val) => _voxels[index.x + index.y * ChunkSize.x + index.z * ChunkSize.x * ChunkSize.y] = val;
        public void SetVoxel(int x, int y, int z, uint val) => _voxels[x + y * ChunkSize.x + z * ChunkSize.x * ChunkSize.y] = val;
        public void SetVoxel(int index, uint val) => _voxels[index] = val;
        public uint GetVoxel(int3 index) => _voxels[index.x + index.y * ChunkSize.x + index.z * ChunkSize.x * ChunkSize.y];
        public uint GetVoxel(int x, int y, int z) => _voxels[x + y * ChunkSize.x + z * ChunkSize.x * ChunkSize.y];
        public uint GetVoxel(int index) => _voxels[index];
        //public uint[] GetVoxels() => _voxels;
        public NativeArray<uint> GetVoxels() => _voxels;
        #endregion

        //Debugging Objects
        public LineRenderer[] borderLines = new LineRenderer[0];

        public async void Startup(ChunkPosition position, bool useThreads = true)
        {
            Initialized = false;

            if (useThreads)
            {
                Task task = Task.Factory.StartNew(() => Setup(position), TaskCreationOptions.LongRunning);

                while (!task.IsCompletedSuccessfully)
                {
                    if (destroyCancellationToken.IsCancellationRequested)
                        return;

                    if (task.Exception != null)
                    {
                        Debug.LogError(task.Exception);
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        Debug.LogError($"Task For Chunk({Position}) Faulted");
                        return;
                    }
                    await Awaitable.NextFrameAsync();
                }

                OnSetupTaskComplete();
            }
            else
            {
                Setup(position);
                OnSetupTaskComplete();
            }
        }

        void Setup(ChunkPosition position)
        {
            Position = position;
            WorldPosition = WorldHelper.ChunkPosToWorldPos(Position);
            WorldSettings_SO settings = EngineSettings.WorldSettings;
            ChunkSize = settings.ChunkSize;

            GenerateTerrain(settings);
            RenderManager.Initialize(this);
            ChunkManager.Chunks.TryAdd(Position, this);
        }

        void OnSetupTaskComplete()
        {
            DebugHelper.OnDebugging += OnDebugging;
            OnDebugging(DebugHelper.Debugging);
            Initialized = true;
            ChunkManager.OnChunkLoaded?.Invoke(Position);

            //TODO:: REMOVE THIS AND CHECK INSTEAD IF THE CHUNK IS AT 0,0, AND THE WORLD IS BEING GENEREATED ON STARTUP
            if (Position == ChunkPosition.Zero)
                GameManager.OnCenterChunkInit();
        }

        void GenerateTerrain(WorldSettings_SO settings)
        {
            _voxels = new NativeArray<uint>(ChunkSize.x * ChunkSize.y * ChunkSize.z, Allocator.Persistent);
            NativeArray<int> heightNoise = new NativeArray<int>(settings.ChunkSize.x * settings.ChunkSize.z, Allocator.TempJob);
            NativeArray<float> heightOctavesOffset = new NativeArray<float>(settings.HeightNoise.Octaves * 2, Allocator.TempJob);
            NativeArray<int> caveNoise = new NativeArray<int>(settings.ChunkSize.x * settings.ChunkSize.y * settings.ChunkSize.z, Allocator.TempJob);
            NativeArray<float> caveOctavesOffset = new NativeArray<float>(settings.CaveNoise.Octaves * 3, Allocator.TempJob);

            JobHandle handle = new GenerateTerrainJob(ref _voxels, settings, WorldPosition, ref heightNoise, ref heightOctavesOffset, ref caveNoise, ref caveOctavesOffset).Schedule();
            handle.Complete();

            heightNoise.Dispose();
            heightOctavesOffset.Dispose();
            caveNoise.Dispose();
            caveOctavesOffset.Dispose();
        }

        public GameObject Dispose(bool isEditor = false)
        {
            DebugHelper.OnDebugging -= OnDebugging;

            if (_voxels.IsCreated)
                _voxels.Dispose();

            ChunkManager.Chunks.TryRemove(Position, out _);

            if (isEditor)
            {
                try
                {
                    GameObject.DestroyImmediate(gameObject);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            else
            {
                GameManager.Instance.Pool.ChunkOBJPool.Release(gameObject);
            }

            return gameObject;
        }

        #region Debugging
        // Gets called when Debugging is toggled
        void OnDebugging(bool isDebugging)
        {
            if (isDebugging)
                InitDebugLines();
            else
                foreach (var line in borderLines)
                    line.enabled = false;
        }

        void InitDebugLines()
        {
            float offset = 0.5f;

            // place the lines at the corners of the chunk
            float x = WorldPosition.x - offset;
            float z = WorldPosition.z - offset;
            borderLines[0].positionCount = 2;
            borderLines[0].SetPositions(new Vector3[] { new Vector3(x, -EngineSettings.WorldSettings.ChunkSize.y, z), new Vector3(x, +EngineSettings.WorldSettings.ChunkSize.y, z) });

            x = WorldPosition.x - offset + EngineSettings.WorldSettings.ChunkSize.x;
            z = WorldPosition.z - offset;
            borderLines[1].positionCount = 2;
            borderLines[1].SetPositions(new Vector3[] { new Vector3(x, -EngineSettings.WorldSettings.ChunkSize.y, z), new Vector3(x, +EngineSettings.WorldSettings.ChunkSize.y, z) });

            x = WorldPosition.x - offset;
            z = WorldPosition.z - offset + EngineSettings.WorldSettings.ChunkSize.z;
            borderLines[2].positionCount = 2;
            borderLines[2].SetPositions(new Vector3[] { new Vector3(x, -EngineSettings.WorldSettings.ChunkSize.y, z), new Vector3(x, +EngineSettings.WorldSettings.ChunkSize.y, z) });

            x = WorldPosition.x - offset + EngineSettings.WorldSettings.ChunkSize.x;
            z = WorldPosition.z - offset + EngineSettings.WorldSettings.ChunkSize.z;
            borderLines[3].positionCount = 2;
            borderLines[3].SetPositions(new Vector3[] { new Vector3(x, -EngineSettings.WorldSettings.ChunkSize.y, z), new Vector3(x, +EngineSettings.WorldSettings.ChunkSize.y, z) });

            foreach (var line in borderLines)
                line.enabled = true;
        }
        #endregion
    }
}