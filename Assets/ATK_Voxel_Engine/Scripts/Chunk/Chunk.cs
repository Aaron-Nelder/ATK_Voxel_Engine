using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public ChunkPosition Position { get; private set; }
    public GameObject Object { get; private set; }
    public bool IsDirty { get; private set; } = true;

    ChunkRenderer chunkRenderer;
    public ChunkRenderer ChunkRenderer => chunkRenderer;

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

    //Noises
    int[,] heightNoise;
    int[,,] caveNoise;

    //Debugging Objects
    public LineRenderer[] borderLines = new LineRenderer[0];

    public Chunk Init(ChunkPosition position, bool listen = true)
    {
        if (!ChunkManager.Chunks.TryAdd(position, this)) 
            return null;

        Voxels = new Dictionary<Vector3Int, uint>();
        this.Position = position;
        Object = transform.gameObject;

        IsListening = listen;

        DebugHelper.OnDebugging += OnDebugging;
        OnDebugging(DebugHelper.Debugging);

        AssignVoxels(VoxelManager.WorldSettings);

#if UNITY_EDITOR
        BenchmarkManager.Bench(() => SetUpMesh(), $"Setting up Mesh:{Position}");
#endif
#if UNITY_STANDALONE
        SetUpMesh();
#endif

        VoxelManager.OnChunkTick += Tick;
        return this;
    }

    void SetUpMesh()
    {
        Renderer.material = VoxelManager.MaterialAtlas[0].Material; // TODO:: set proper material

        Mesh = new Mesh();
        Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Mesh.MarkDynamic();
        Filter.sharedMesh = Mesh;
        Collider.sharedMesh = Mesh;

        chunkRenderer = new ChunkRenderer(this);
        IsDirty = !ChunkRenderer.ApplyData(Filter, Collider);      
    }

    void Tick()
    {
        if (IsDirty)
            IsDirty = !ChunkRenderer.ApplyData(Filter, Collider);
    }

    void AssignVoxels(WorldSettings_SO worldSettings)
    {
        int chunkSize = worldSettings.chunkSize;
        int worldHeight = worldSettings.worldHeight;
        int offsetX = Position.x * worldSettings.chunkSize;
        int offsetZ = Position.z * worldSettings.chunkSize;
        heightNoise = NoiseGenerator.GetNoiseMap(worldSettings.seed, chunkSize, worldSettings.HeightNoise.MagClamp, worldSettings.HeightNoise.Octaves, worldSettings.HeightNoise.Persistence, worldSettings.HeightNoise.Lacunarity, new Vector2(offsetX, offsetZ), worldHeight);
        //caveNoise = NoiseGenerator.GenerateCaveMap(chunkSize, worldHeight, chunkSize, new Vector2(offsetX, offsetZ));

        for (int x = 0; x < chunkSize; x++)
            for (int y = -worldHeight; y < worldHeight; y++)
                for (int z = 0; z < chunkSize; z++)
                    AddVoxel(new(x, y, z), ref worldHeight);
    }

    void AddVoxel(Vector3Int pos, ref int worldHeight)
    {
        uint id = GetVoxelId(ref pos, ref worldHeight);
        Voxels.Add(pos, id);
    }

    // returns the block ID for the given position
    uint GetVoxelId(ref Vector3Int pos, ref int worldHeight)
    {
        int surfaceHeight = heightNoise[pos.x, pos.z];
        //int caveHeight = caveNoise[pos.x, pos.y + worldHeight, pos.z];

        /*
        // Set the air voxels above the surface
        if (pos.y > surfaceHeight)
            return 0;
     
        // Checks for surface blocks
        else if (pos.y <= surfaceHeight && pos.y >= surfaceHeight - 3)
        {
            return (uint)caveHeight;
        }
        else if(pos.y == -worldHeight)
        {
            return 2;
        }
        else
        {
            if ((uint)caveHeight == 1)
                return 2;
            return 0;
        }
        */

        if (pos.y <= surfaceHeight && pos.y >= surfaceHeight - 3)
            return 1;
        else if (pos.y > surfaceHeight)
            return 0;
        else
            return 2;

    }

    // Sets the given voxel to air and surounding voxels to display their proper faces
    public void DestroyVoxel(Vector3Int localPos)
    {
        int worldHeight = VoxelManager.WorldSettings.worldHeight;
        if (localPos.y <= -worldHeight || localPos.y > worldHeight)
            return;

        // sets the current voxel to air at the given world position
        ChunkRenderer.RemoveData(this, localPos);
        Voxels[localPos] = 0;

        int chunkSize = VoxelManager.WorldSettings.chunkSize;

        // loops through all directions and adds the new visible faces to the surrounding voxels
        foreach (var dir in WorldHelper.Directions)
        {
            Vector3Int checkPos = localPos + dir;
            ChunkPosition chunkPos = Position;

            // If the voxel position is not in the chunk increase the chunk position
            if (checkPos.x < 0)
            {
                chunkPos.x--;
                checkPos.x += chunkSize;
            }
            else if (checkPos.x >= chunkSize)
            {
                chunkPos.x++;
                checkPos.x -= chunkSize;
            }

            if (checkPos.z < 0)
            {
                chunkPos.z--;
                checkPos.z += chunkSize;
            }
            else if (checkPos.z >= chunkSize)
            {
                chunkPos.z++;
                checkPos.z -= chunkSize;
            }

            if (checkPos.y >= -worldHeight && checkPos.y < worldHeight)
                ChunkManager.Chunks[chunkPos].ChunkRenderer.AddVoxelFace(ChunkManager.Chunks[chunkPos], checkPos, -dir);
        }

#if UNITY_EDITOR
        BenchmarkManager.Bench(() => IsDirty = !ChunkRenderer.ApplyData(Filter, Collider), $"Destroying Voxel:{localPos}");
#endif
#if UNITY_STANDALONE
        IsDirty = !ChunkRenderer.ApplyData(Filter, Collider);
#endif
    }

    public void PlaceVoxel(Vector3Int voxelPos, uint id)
    {
        int worldHeight = VoxelManager.WorldSettings.worldHeight;
        if (voxelPos.y <= -worldHeight || voxelPos.y >= worldHeight) return;

        Voxels[voxelPos] = id;
        ChunkRenderer.RefreshVoxel(this, voxelPos);

        int chunkSize = VoxelManager.WorldSettings.chunkSize;

        // loops through all directions and adds the new visible faces to the surrounding voxels
        foreach (var dir in WorldHelper.Directions)
        {
            Vector3Int checkPos = voxelPos + dir;
            ChunkPosition chunkPos = Position;

            // if the voxel position is not in the chunk increase the chunk position
            if (checkPos.x < 0)
            {
                chunkPos.x--;
                checkPos.x += chunkSize;
            }
            else if (checkPos.x >= chunkSize)
            {
                chunkPos.x++;
                checkPos.x -= chunkSize;
            }

            if (checkPos.z < 0)
            {
                chunkPos.z--;
                checkPos.z += chunkSize;
            }
            else if (checkPos.z >= chunkSize)
            {
                chunkPos.z++;
                checkPos.z -= chunkSize;
            }

            if (checkPos.y >= -worldHeight)
                ChunkManager.Chunks[chunkPos].ChunkRenderer.RefreshVoxel(ChunkManager.Chunks[chunkPos], checkPos);
        }

        IsDirty = !ChunkRenderer.ApplyData(Filter, Collider);
    }

    void OnChunkLoaded(Chunk chunk)
    {
        // If the chunk is a neighbour of the loaded chunk, update the visible blocks
        if (chunk.Position.x + 1 == Position.x || chunk.Position.x - 1 == Position.x || chunk.Position.z + 1 == Position.z || chunk.Position.z - 1 == Position.z)
        {
            //if (IsDirty)
            {
                ChunkRenderer.RefreshBorderVoxels(this);
                //ChunkRenderer.RefreshVisibleVoxels(this);
                IsDirty = !ChunkRenderer.ApplyData(Filter, Collider);
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
            GameObject.DestroyImmediate(Object);
        else
            GameObject.Destroy(Object);
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
            LineRenderer l = GameObject.Instantiate(VoxelManager.DebugSettings.debugLinePrefab, Object.transform).GetComponent<LineRenderer>();
            l.transform.SetAsFirstSibling();
            borderLines[i] = l;
        }

        Vector3Int pos = WorldHelper.ChunkPosToWorldPos(Position);

        int chunkSize = VoxelManager.WorldSettings.chunkSize;
        int worldHeight = VoxelManager.WorldSettings.worldHeight;
        float offset = 0.5f;

        // place the lines at the corners of the chunk
        float x = pos.x - offset;
        float z = pos.z - offset;
        borderLines[0].positionCount = 2;
        borderLines[0].SetPositions(new Vector3[] { new Vector3(x, -worldHeight, z), new Vector3(x, +worldHeight, z) });

        x = pos.x - offset + chunkSize;
        z = pos.z - offset;
        borderLines[1].positionCount = 2;
        borderLines[1].SetPositions(new Vector3[] { new Vector3(x, -worldHeight, z), new Vector3(x, +worldHeight, z) });


        x = pos.x - offset;
        z = pos.z - offset + chunkSize;
        borderLines[2].positionCount = 2;
        borderLines[2].SetPositions(new Vector3[] { new Vector3(x, -worldHeight, z), new Vector3(x, +worldHeight, z) });

        x = pos.x - offset + chunkSize;
        z = pos.z - offset + chunkSize;
        borderLines[3].positionCount = 2;
        borderLines[3].SetPositions(new Vector3[] { new Vector3(x, -worldHeight, z), new Vector3(x, +worldHeight, z) });
    }

    #endregion
}