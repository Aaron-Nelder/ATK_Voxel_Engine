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
        ChunkData _data;
        public ref ChunkData Data => ref _data;
        public bool HasData { get; private set; } = false;
        [field: SerializeField] public ChunkRenderManager RenderManager { get; private set; }

        //Debugging Objects
        public LineRenderer[] borderLines = new LineRenderer[0];

        public async void Initialize(ChunkPosition position)
        {
            // this is done so we aren't creating a new memory allocation for the data
            if (!HasData)
                _data = new ChunkData(position, EngineUtilities.ChunkPosToWorldPosInt3(position), EngineSettings.WorldSettings.ChunkSize);
            else
                _data.SetPositionAndSize(position, EngineUtilities.ChunkPosToWorldPosInt3(position), EngineSettings.WorldSettings.ChunkSize);

            if (!ChunkManager.Chunks.TryAdd(position, this))
                return;

            await DetermineBiome(position);
            await GenerateTerrain(EngineSettings.WorldSettings);

            _data.SetState(ChunkState.LOADING_MESH);
            await RenderManager.Initialize(this);

            DebugHelper.OnDebugging += OnDebugging;
            OnDebugging(DebugHelper.Debugging);

            HasData = true;

            _data.SetState(ChunkState.LOADED);
            ChunkLoadManager.OnLoaded(this);
        }

        async Task DetermineBiome(ChunkPosition position)
        {
            _data.SetState(ChunkState.DETERMINING_BIOME);
            Task task = Task.Factory.StartNew(
                () =>
                {
                    Data.SetBiome(BiomeType.PLAINS);
                }
            , TaskCreationOptions.None);
            await TaskUtility.AwaitTask(task, destroyCancellationToken);
        }

        async Task GenerateTerrain(WorldSettings_SO settings)
        {
            Task task = Task.Factory.StartNew(
                () =>
                {
                    BiomeData_SO biomeData = Data.BiomeData;

                    // surface
                    _data.SetState(ChunkState.GENERATING_SURFACE);
                    NativeArray<int> heightNoise = new NativeArray<int>(settings.ChunkSize.x * settings.ChunkSize.z, Allocator.TempJob);
                    NativeArray<float> heightOctavesOffset = new NativeArray<float>(biomeData.HeightNoise.Octaves * 2, Allocator.TempJob);
                    JobHandle surfaceHandle = new NoiseJob(settings, biomeData.HeightNoise, Data.WorldPosition, heightNoise, heightOctavesOffset).Schedule();

                    // caves
                    _data.SetState(ChunkState.GENERATING_CAVES);
                    NativeArray<int> caveNoise = new NativeArray<int>(settings.ChunkSize.x * settings.ChunkSize.y * settings.ChunkSize.z, Allocator.TempJob);
                    NativeArray<float> caveOctavesOffset = new NativeArray<float>(biomeData.CaveNoise.Octaves * 3, Allocator.TempJob);
                    JobHandle caveHandle = new NoiseJob(settings, biomeData.CaveNoise, Data.WorldPosition, caveNoise, caveOctavesOffset).Schedule();

                    // assigning voxels
                    _data.SetState(ChunkState.ASSIGNING_VOXELS);
                    JobHandle dependicies = JobHandle.CombineDependencies(surfaceHandle, caveHandle);
                    JobHandle assignVoxelsHandle = new AssignVoxelsJob(Data.Voxels, settings.ChunkSize, heightNoise, caveNoise).Schedule(dependicies);
                    assignVoxelsHandle.Complete();

                    caveNoise.Dispose();
                    caveOctavesOffset.Dispose();

                    // folliageNoise
                    NativeArray<int> folliageNoise = new NativeArray<int>(settings.ChunkSize.x * settings.ChunkSize.y * settings.ChunkSize.z, Allocator.TempJob);
                    NativeArray<float> folliageOctavesOffset = new NativeArray<float>(biomeData.FolliageNoise.Octaves * 2, Allocator.TempJob);
                    JobHandle folliageNoiseHandle = new NoiseJob(settings, biomeData.FolliageNoise, Data.WorldPosition, folliageNoise, folliageOctavesOffset).Schedule();

                    // folliage
                    _data.SetState(ChunkState.GENERATING_FOLLIAGE);
                    NativeArray<Folliage> folliages = new NativeArray<Folliage>(biomeData.folliages.Length, Allocator.TempJob);
                    for (int i = 0; i < biomeData.folliages.Length; i++)
                        folliages[i] = new Folliage(biomeData.folliages[i].type, biomeData.folliages[i].density);
                    JobHandle folliageHandle = new GenerateFolliageJob(settings.Seed, _data.Position, folliages, Data.Voxels, heightNoise, folliageNoise, settings.ChunkSize).Schedule(folliageNoiseHandle);
                    folliageHandle.Complete();

                    folliageNoise.Dispose();
                    folliageOctavesOffset.Dispose();
                    folliages.Dispose();
                    heightNoise.Dispose();
                    heightOctavesOffset.Dispose();
                }
            , TaskCreationOptions.LongRunning);
            await TaskUtility.AwaitTask(task, destroyCancellationToken);
        }

        public void Dispose()
        {
            _data.SetState(ChunkState.UNLOADING);
            DebugHelper.OnDebugging -= OnDebugging;
            ChunkManager.Chunks.TryRemove(_data.Position, out _);


            RenderManager.Dispose();
            _data.SetState(ChunkState.NULL);

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                DestroyImmediate(gameObject);
                return;
            }
#endif
            EngineManager.Instance.Pool.ChunkOBJPool.Release(gameObject);
        }

        void OnDestroy() => Data.Dispose();

        #region Debugging
        // Gets called when Debugging is toggled
        void OnDebugging(bool isDebugging)
        {
            if (isDebugging && _debugLinesSet)
                foreach (var line in borderLines)
                    line.gameObject.SetActive(true);
            else if (isDebugging)
                InitDebugLines();
            else
                foreach (var line in borderLines)
                    line.gameObject.SetActive(false);
        }

        bool _debugLinesSet = false;
        void InitDebugLines()
        {
            // place the lines at the corners of the chunk
            float x = _data.WorldPosition.x - EngineConstants.DEBUG_LINE_OFFSET;
            float z = _data.WorldPosition.z - EngineConstants.DEBUG_LINE_OFFSET;
            borderLines[0].positionCount = 2;
            borderLines[0].SetPositions(new Vector3[] { new Vector3(x, -_data.ChunkSize.y, z), new Vector3(x, +_data.ChunkSize.y, z) });

            x = _data.WorldPosition.x - EngineConstants.DEBUG_LINE_OFFSET + _data.ChunkSize.x;
            z = _data.WorldPosition.z - EngineConstants.DEBUG_LINE_OFFSET;
            borderLines[1].positionCount = 2;
            borderLines[1].SetPositions(new Vector3[] { new Vector3(x, -_data.ChunkSize.y, z), new Vector3(x, +_data.ChunkSize.y, z) });

            x = _data.WorldPosition.x - EngineConstants.DEBUG_LINE_OFFSET;
            z = _data.WorldPosition.z - EngineConstants.DEBUG_LINE_OFFSET + _data.ChunkSize.z;
            borderLines[2].positionCount = 2;
            borderLines[2].SetPositions(new Vector3[] { new Vector3(x, -_data.ChunkSize.y, z), new Vector3(x, +_data.ChunkSize.y, z) });

            x = _data.WorldPosition.x - EngineConstants.DEBUG_LINE_OFFSET + _data.ChunkSize.x;
            z = _data.WorldPosition.z - EngineConstants.DEBUG_LINE_OFFSET + _data.ChunkSize.z;
            borderLines[3].positionCount = 2;
            borderLines[3].SetPositions(new Vector3[] { new Vector3(x, -_data.ChunkSize.y, z), new Vector3(x, +_data.ChunkSize.y, z) });

            foreach (var line in borderLines)
                line.gameObject.SetActive(true);

            _debugLinesSet = true;
        }
        #endregion
    }

    // Stores the data for a chunk
    public struct ChunkData : IDisposable
    {
        #region Variables
        ChunkPosition _pos;
        int3 _wPos;
        int3 _chunkSize;
        BiomeType _biome;
        NativeArray<VoxelType> _voxels;
        ChunkState _state;
        #endregion

        #region Properties
        public ChunkPosition Position => _pos;
        public int3 WorldPosition => _wPos;
        public int3 ChunkSize => _chunkSize;
        public BiomeType Biome => _biome;
        public BiomeData_SO BiomeData
        {
            get
            {
                if (_biome == BiomeType.NULL)
                {
                    ScreenLogger.Log(LogType.Warning, "Cannot get biome data for NULL biome");
                    return null;
                }
                return EngineSettings.GetBiomeData(_biome);
            }
        }
        public ChunkState State => _state;
        #endregion

        #region Voxel Get/Set
        public void SetVoxel(int3 index, VoxelType val) => _voxels[index.x + index.y * ChunkSize.x + index.z * ChunkSize.x * ChunkSize.y] = val;
        public void SetVoxel(int x, int y, int z, VoxelType val) => _voxels[x + y * ChunkSize.x + z * ChunkSize.x * ChunkSize.y] = val;
        public void SetVoxel(int index, VoxelType val) => _voxels[index] = val;
        public VoxelType GetVoxel(int3 index) => (VoxelType)_voxels[index.x + index.y * ChunkSize.x + index.z * ChunkSize.x * ChunkSize.y];
        public VoxelType GetVoxel(int x, int y, int z) => (VoxelType)_voxels[x + y * ChunkSize.x + z * ChunkSize.x * ChunkSize.y];
        public VoxelType GetVoxel(int index) => (VoxelType)_voxels[index];
        public NativeArray<VoxelType> Voxels => _voxels;
        #endregion

        #region Public Functions
        public ChunkData(ChunkPosition position, int3 worldPosition, int3 chunkSize)
        {
            _pos = position;
            _wPos = worldPosition;
            _chunkSize = chunkSize;
            _biome = BiomeType.NULL;
            _state = ChunkState.NULL;
            _voxels = new NativeArray<VoxelType>(chunkSize.x * chunkSize.y * chunkSize.z, Allocator.Persistent);
        }

        // Sets the position and size of the chunk while allocating memory for the voxel array
        public void SetPositionAndSize(ChunkPosition pos, int3 wPos, int3 chunkSize)
        {
            _pos = pos;
            _wPos = wPos;
            _chunkSize = chunkSize;
            _state = ChunkState.NULL;
            AllocateVoxelMemory();
        }

        // Sets the Biome as long as it's not NULL
        public void SetBiome(BiomeType b)
        {
            if (b == BiomeType.NULL)
            {
                ScreenLogger.Log(LogType.Warning, "Cannot set biome to NULL");
                return;
            }
            _biome = b;
        }

        public void SetState(ChunkState state) => _state = state;

        // Disposes of the voxel array and resets the chunk data
        public void Dispose()
        {
            _pos = ChunkPosition.zero;
            _wPos = int3.zero;
            _chunkSize = int3.zero;
            _biome = BiomeType.NULL;

            if (_voxels.IsCreated)
                _voxels.Dispose();
        }
        #endregion

        #region Private Functions
        // Allocates memory for the voxel array
        void AllocateVoxelMemory()
        {
            if (_voxels.IsCreated)
                _voxels.Dispose();

            _voxels = new NativeArray<VoxelType>(ChunkSize.x * ChunkSize.y * ChunkSize.z, Allocator.Persistent);
        }
        #endregion
    }
}