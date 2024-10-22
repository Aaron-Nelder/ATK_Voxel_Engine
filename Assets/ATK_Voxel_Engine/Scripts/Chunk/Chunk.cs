using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.LowLevelPhysics;

namespace ATKVoxelEngine
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        public ChunkPosition Position { get; private set; }
        public Vector3Int WorldPosition { get; private set; }
        public bool IsDirty { get; private set; } = true;
        public ChunkRenderer ChunkRenderer { get; private set; }

        // Voxels
        public Dictionary<Vector3Int, uint> Voxels { get; private set; }
        public Bounds Bounds => Renderer.bounds;

        [field: SerializeField] public MeshFilter Filter { get; private set; }
        [field: SerializeField] public MeshRenderer Renderer { get; private set; }
        [field: SerializeField] public MeshCollider Collider { get; private set; }
        public Mesh Mesh { get; private set; }

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

        public void MarkDirty() => IsDirty = true;
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
            WorldSettings_SO settings = VoxelManager.WorldSettings;
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
            VoxelManager.OnChunkTick += Tick;

            //TODO:: REMOVE THIS AND CHECK INSTEAD IF THE CHUNK IS AT 0,0, AND THE WORLD IS BEING GENEREATED ON STARTUP
            if (Position == ChunkPosition.Zero)
                VoxelManager.OnCenterChunkInit();
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
            _caveNoise = NoiseGenerator.GetNoise3D(worldSettings, worldSettings.CaveNoise, WorldPosition.x, WorldPosition.z);
        }

        void SetUpMesh()
        {
            Renderer.material = VoxelManager.MaterialAtlas[0].Material; // TODO:: set proper material
            Mesh = new Mesh();
            Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Mesh.MarkDynamic();
            Filter.sharedMesh = Mesh;
            Collider.sharedMesh = Mesh;
            ChunkRenderer.ApplyData(Filter, Collider, () => IsDirty = false);
        }

        void Tick()
        {
            if (IsDirty)
                ChunkRenderer.ApplyData(Filter, Collider, () => IsDirty = false);
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
            int caveHeight = _caveNoise[pos.x, pos.y, pos.z];

            bool isVoxel = caveHeight != 0;

            // BEDROCK
            if (pos.y == 0)
                return 2;

            if (surfaceHeight - pos.y < 3 && isVoxel)
                return 1;

            return (uint)caveHeight * 2;
        }

        // Sets the given voxel to air and surounding voxels to display their proper faces
        public void DestroyVoxel(Vector3Int voxelPos)
        {
            if (!Initialized) return;

            int chunkSizeX = VoxelManager.WorldSettings.ChunkSize.x;
            int chunkSizeY = VoxelManager.WorldSettings.ChunkSize.y;
            int chunkSizeZ = VoxelManager.WorldSettings.ChunkSize.z;

            if (!WorldHelper.VoxelInBounds(voxelPos.y, chunkSizeY)) return;

            // sets the current voxel to air at the given world position
            ChunkRenderer.ClearData(this, voxelPos);
            Voxels[voxelPos] = 0;

            // loops through all directions and adds the new visible faces to the surrounding voxels
            foreach (var dir in WorldHelper.Directions)
            {
                Vector3Int checkPos = voxelPos + dir;
                ChunkPosition chunkPos = Position;

                // If the voxel position is not in the chunk increase the chunk position
                if (checkPos.x < 0)
                {
                    chunkPos.x--;
                    checkPos.x += chunkSizeX;
                }
                else if (checkPos.x >= chunkSizeX)
                {
                    chunkPos.x++;
                    checkPos.x -= chunkSizeX;
                }

                if (checkPos.z < 0)
                {
                    chunkPos.z--;
                    checkPos.z += chunkSizeZ;
                }
                else if (checkPos.z >= chunkSizeZ)
                {
                    chunkPos.z++;
                    checkPos.z -= chunkSizeZ;
                }

                if (WorldHelper.VoxelInBounds(checkPos.y, chunkSizeY))
                    ChunkManager.Chunks[chunkPos].ChunkRenderer.AddVoxelFace(ChunkManager.Chunks[chunkPos], checkPos, -dir);
            }

            ChunkRenderer.ApplyData(Filter, Collider, () => IsDirty = false);
        }

        public void PlaceVoxel(Vector3Int voxelPos, uint id)
        {
            if (!Initialized) return;

            int chunkSizeX = VoxelManager.WorldSettings.ChunkSize.x;
            int chunkSizeY = VoxelManager.WorldSettings.ChunkSize.y;
            int chunkSizeZ = VoxelManager.WorldSettings.ChunkSize.z;

            if (!WorldHelper.VoxelInBounds(voxelPos.y, chunkSizeY)) return;

            Voxels[voxelPos] = id;
            ChunkRenderer.AddVisibleFaces(this, voxelPos);

            // loops through all directions and adds the new visible faces to the surrounding voxels
            foreach (var dir in WorldHelper.Directions)
            {
                Vector3Int checkPos = voxelPos + dir;
                ChunkPosition chunkPos = Position;

                // if the voxel position is not in the chunk increase the chunk position
                if (checkPos.x < 0)
                {
                    chunkPos.x--;
                    checkPos.x += chunkSizeX;
                }
                else if (checkPos.x >= chunkSizeX)
                {
                    chunkPos.x++;
                    checkPos.x -= chunkSizeX;
                }

                if (checkPos.z < 0)
                {
                    chunkPos.z--;
                    checkPos.z += chunkSizeZ;
                }
                else if (checkPos.z >= chunkSizeZ)
                {
                    chunkPos.z++;
                    checkPos.z -= chunkSizeZ;
                }

                if (WorldHelper.VoxelInBounds(checkPos.y, chunkSizeY))
                    ChunkManager.Chunks[chunkPos].ChunkRenderer.AddVisibleFaces(ChunkManager.Chunks[chunkPos], checkPos);
            }

            ChunkRenderer.ApplyData(Filter, Collider, () => IsDirty = false);
        }

        void OnChunkLoaded(Chunk chunk)
        {
            if (!Initialized) return;

            // If the chunk is a neighbour of the loaded chunk, update the visible blocks
            if (chunk.Position.x + 1 == Position.x || chunk.Position.x - 1 == Position.x || chunk.Position.z + 1 == Position.z || chunk.Position.z - 1 == Position.z)
            {
                if (IsDirty)
                {
                    //ChunkRenderer.RefreshBorderVoxels(this);
                    ChunkRenderer.RefreshVisibleVoxels(this);
                    ChunkRenderer.ApplyData(Filter, Collider, () => IsDirty = false);
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
            VoxelManager.OnChunkTick -= Tick;

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
            borderLines = new LineRenderer[4];
            for (int i = 0; i < 4; i++)
            {
                LineRenderer l = GameObject.Instantiate(VoxelManager.DebugSettings.debugLinePrefab, gameObject.transform).GetComponent<LineRenderer>();
                l.transform.SetAsFirstSibling();
                borderLines[i] = l;
            }

            Vector3Int pos = WorldHelper.ChunkPosToWorldPos(Position);

            float offset = 0.5f;

            // place the lines at the corners of the chunk
            float x = pos.x - offset;
            float z = pos.z - offset;
            borderLines[0].positionCount = 2;
            borderLines[0].SetPositions(new Vector3[] { new Vector3(x, -VoxelManager.WorldSettings.ChunkSize.y, z), new Vector3(x, +VoxelManager.WorldSettings.ChunkSize.y, z) });

            x = pos.x - offset + VoxelManager.WorldSettings.ChunkSize.x;
            z = pos.z - offset;
            borderLines[1].positionCount = 2;
            borderLines[1].SetPositions(new Vector3[] { new Vector3(x, -VoxelManager.WorldSettings.ChunkSize.y, z), new Vector3(x, +VoxelManager.WorldSettings.ChunkSize.y, z) });


            x = pos.x - offset;
            z = pos.z - offset + VoxelManager.WorldSettings.ChunkSize.z;
            borderLines[2].positionCount = 2;
            borderLines[2].SetPositions(new Vector3[] { new Vector3(x, -VoxelManager.WorldSettings.ChunkSize.y, z), new Vector3(x, +VoxelManager.WorldSettings.ChunkSize.y, z) });

            x = pos.x - offset + VoxelManager.WorldSettings.ChunkSize.x;
            z = pos.z - offset + VoxelManager.WorldSettings.ChunkSize.z;
            borderLines[3].positionCount = 2;
            borderLines[3].SetPositions(new Vector3[] { new Vector3(x, -VoxelManager.WorldSettings.ChunkSize.y, z), new Vector3(x, +VoxelManager.WorldSettings.ChunkSize.y, z) });
        }

        #endregion
    }
}