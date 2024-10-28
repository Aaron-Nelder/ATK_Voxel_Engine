using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace ATKVoxelEngine
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        public ChunkPosition Position { get; private set; }
        public Vector3Int WorldPosition { get; private set; }
        public bool IsDirty { get; set; } = true;
        public ChunkRenderer ChunkRenderer { get; private set; }

        // Voxels
        public Dictionary<Vector3Int, uint> Voxels { get; private set; }
        public Bounds Bounds => Renderer.bounds;

        [field: SerializeField] public MeshFilter Filter { get; private set; }
        [field: SerializeField] public MeshRenderer Renderer { get; private set; }
        [field: SerializeField] public MeshCollider Collider { get; private set; }
        Mesh _mesh;

        bool isListening = false;
        public bool IsListening
        {
            get => isListening;
            set
            {
                isListening = value;

                if (value)
                    ChunkManager.OnChunkLoaded += OnChunkLoaded;
                else
                    ChunkManager.OnChunkLoaded -= OnChunkLoaded;

                IsDirty = value;
            }
        }

        public bool Initialized { get; private set; }

        //Noisess
        int[,] _heightNoise;
        int[,,] _caveNoise;

        //Debugging Objects
        public LineRenderer[] borderLines = new LineRenderer[0];

        Coroutine _setupChunkTask;

        public void Startup(ChunkPosition position, bool listen = true, bool isEditor = false)
        {
            Initialized = false;
            if (ChunkManager.Chunks.ContainsKey(position)) return;

            if (isEditor)
            {
                Setup(position, listen);
                OnSetupTaskComplete();
                return;
            }

            if (_setupChunkTask != null) StopCoroutine(_setupChunkTask);
            _setupChunkTask = StartCoroutine(SetupChunkTask(position, listen, () => OnSetupTaskComplete()));
        }

        IEnumerator SetupChunkTask(ChunkPosition position, bool listen = true, Action callback = null)
        {
            Task task = Task.Factory.StartNew(() => Setup(position, listen), TaskCreationOptions.LongRunning);

            while (!task.IsCompletedSuccessfully)
            {
                if (task.Exception != null)
                {
                    UnityEngine.Debug.LogError(task.Exception);
                    break;
                }

                if (task.IsFaulted)
                {
                    UnityEngine.Debug.LogError($"Task For Chunk({Position}) Faulted");
                    break;
                }

                yield return null;
            }

            callback?.Invoke();
            _setupChunkTask = null;
        }

        void Setup(ChunkPosition position, bool listen = true)
        {
            IsListening = listen;
            Position = position;
            WorldPosition = WorldHelper.ChunkPosToWorldPos(Position);
            WorldSettings_SO settings = EngineSettings.WorldSettings;
            FillChunk(settings);
            GetNoise(settings);
            AssignVoxels(settings);
            ChunkManager.Chunks.TryAdd(Position, this);
            ChunkRenderer = new ChunkRenderer(this);
        }

        void OnSetupTaskComplete()
        {
            DebugHelper.OnDebugging += OnDebugging;
            OnDebugging(DebugHelper.Debugging);
            SetUpMesh();
            Initialized = true;
            GameManager.OnChunkTick += Tick;
            ChunkManager.OnChunkLoaded?.Invoke(Position);

            //TODO:: REMOVE THIS AND CHECK INSTEAD IF THE CHUNK IS AT 0,0, AND THE WORLD IS BEING GENEREATED ON STARTUP
            if (Position == ChunkPosition.Zero)
                GameManager.OnCenterChunkInit();
        }

        // Fills all voxels with air
        void FillChunk(WorldSettings_SO worldSettings)
        {
            Voxels = new Dictionary<Vector3Int, uint>();
            for (int x = 0; x < worldSettings.ChunkSize.x; x++)
                for (int y = 0; y < worldSettings.ChunkSize.y; y++)
                    for (int z = 0; z < worldSettings.ChunkSize.z; z++)
                        Voxels.Add(new Vector3Int(x, y, z), 0);
        }

        void GetNoise(WorldSettings_SO worldSettings)
        {
            _heightNoise = NoiseGenerator.GetNoise2D(worldSettings, worldSettings.HeightNoise, WorldPosition.x, WorldPosition.z);
            //_caveNoise = NoiseGenerator.GetNoise3D(worldSettings, worldSettings.CaveNoise, WorldPosition.x, WorldPosition.z);
        }

        void SetUpMesh()
        {
            Renderer.material = EngineSettings.MaterialAtlas[0].Material; // TODO:: set proper material
            _mesh = new Mesh();
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            _mesh.MarkDynamic();
            Filter.sharedMesh = _mesh;
            Collider.sharedMesh = _mesh;
            ChunkRenderer.ApplyData(this);
        }

        void Tick()
        {
            if (IsDirty)
                ChunkRenderer.ApplyData(this);
        }

        void AssignVoxels(WorldSettings_SO worldSettings)
        {
            for (int x = 0; x < worldSettings.ChunkSize.x; x++)
            {
                for (int z = 0; z < worldSettings.ChunkSize.z; z++)
                {
                    int surfaceHeight = _heightNoise[x, z];
                    for (int y = surfaceHeight; y >= 0; y--)
                    {
                        Vector3Int pos = new(x, y, z);
                        Voxels[pos] = GetVoxelId(pos, surfaceHeight);
                    }
                }
            }
        }

        // returns the block ID for the given position
        uint GetVoxelId(Vector3Int pos, int surfaceHeight)
        {
            //int caveHeight = _caveNoise[pos.x, pos.y, pos.z];

            //bool isVoxel = caveHeight != 0;

            // BEDROCK
            if (pos.y == 0)
                return 2;

            if (surfaceHeight - pos.y < 3) //&& isVoxel)
                return 1;

            return 2;//(uint)caveHeight * 2;
        }

        void OnChunkLoaded(ChunkPosition posOfChunk)
        {
            if (!Initialized) return;

            // If the chunk is a neighbour of the loaded chunk, update the visible blocks
            if (posOfChunk.x + 1 == Position.x || posOfChunk.x - 1 == Position.x || posOfChunk.z + 1 == Position.z || posOfChunk.z - 1 == Position.z)
            {
                if (IsDirty)
                {
                    //ChunkRenderer.RefreshBorderVoxels(this);
                    ChunkRenderer.RefreshVisibleVoxels(this, true);
                }
            }
        }

        public void Dispose(bool isEditor = false)
        {
            if (Voxels != null || Voxels.Count >= 0)
            {
                Voxels.Clear();
            }

            IsListening = false;
            IsDirty = false;
            DebugHelper.OnDebugging -= OnDebugging;
            GameManager.OnChunkTick -= Tick;

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
                GameObject.Destroy(gameObject);
        }

        #region Debugging
        // Gets called when Debugging is toggled
        void OnDebugging(bool isDebugging)
        {
            if (isDebugging && borderLines.Length == 0)
                InitDebugLines();

            foreach (var line in borderLines)
                line.enabled = isDebugging;
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
        }

        #endregion
    }
}