using System;
using System.Collections.Concurrent;
using UnityEngine;

public static class ChunkManager
{
    static Transform chunkParent;

    static GameObject ChunkPrefab => VoxelManager.WorldSettings.chunkPrefab;

    #region Dictionary
    public static ConcurrentDictionary<ChunkPosition, Chunk> Chunks { get; private set; } = new ConcurrentDictionary<ChunkPosition, Chunk>();
    #endregion

    public static event Action<Chunk> OnChunkLoaded;
    public static event Action<ChunkPosition, ChunkPosition> OnPlayerChunkUpdate;
    public static void IvokeOnPlayerChunkUpdate(ChunkPosition playerChunk, ChunkPosition dir) => OnPlayerChunkUpdate?.Invoke(playerChunk, dir);

    static bool allChunksListening;
    public static bool AllChunksListening
    {
        get => allChunksListening;
        set
        {
            allChunksListening = value;
            foreach (var chunk in Chunks)
                chunk.Value.IsListening = value;
        }
    }

    public static void SpawnStartingChunks(Transform parent, bool isEditor = false)
    {
        chunkParent = parent;
        Dispose(isEditor);

        GenerateChunksAtOrigin();        

        AllChunksListening = true;
        InvokeOnLoadForAllChunks();

        OnPlayerChunkUpdate += OnPlayerChunkChange;
    }

    static void GenerateChunk(ChunkPosition pos, bool listening = true)
    {
        if (Chunks.ContainsKey(pos)) return;

        GameObject chunkObj = GameObject.Instantiate(ChunkPrefab, WorldHelper.ChunkPosToWorldPos(pos), Quaternion.identity, chunkParent);
        chunkObj.name = $"Chunk: ({pos.x},{pos.z})";
        Chunk chunk = chunkObj.GetComponent<Chunk>().Init(pos, listening);
        OnChunkLoaded?.Invoke(chunk);
    }

    static void InvokeOnLoadForAllChunks()
    {
        foreach (var chunk in Chunks)
            OnChunkLoaded?.Invoke(chunk.Value);
    }

    static void GenerateChunksAtOrigin()
    {
        int interval = VoxelManager.WorldSettings.renderDistanceInChunks / 2;
        for (int x = -interval; x < interval + 1; x += 1)
            for (int z = -interval; z < interval + 1; z += 1)
                GenerateChunk(new(x, z), false);
    }

    static void OnPlayerChunkChange(ChunkPosition playerChunk, ChunkPosition dir)
    {
        LoadChunks(playerChunk, dir);
        UnloadChunks(playerChunk, dir);
    }

    static void LoadChunks(ChunkPosition playerChunk, ChunkPosition dir)
    {
        int halfRenderDistance = VoxelManager.WorldSettings.renderDistanceInChunks / 2;

        ChunkPosition newDir = dir * halfRenderDistance;

        for (int row = -halfRenderDistance; row < halfRenderDistance + 1; row++)
        {
            if (newDir.x == 0)
            {
                int z = newDir.z > 0 ? newDir.z - 1 : newDir.z + 1;
                Chunks[new(playerChunk.x + row, playerChunk.z + z)].MarkDirty();
                GenerateChunk(new(playerChunk.x + row, playerChunk.z + newDir.z));
            }
            else
            {
                int x = newDir.x > 0 ? newDir.x - 1 : newDir.x + 1;
                Chunks[new(playerChunk.x + x, playerChunk.z + row)].MarkDirty();
                GenerateChunk(new(playerChunk.x + newDir.x, playerChunk.z + row));
            }
        }
    }

    // Unload chunks that are not in the render distance
    static void UnloadChunks(ChunkPosition playerChunk, ChunkPosition dir)
    {
        int halfRenderDistance = VoxelManager.WorldSettings.renderDistanceInChunks / 2;

        dir = -dir * halfRenderDistance;

        ChunkPosition startDir = new(dir.x, dir.z);

        if (dir.x != 0)
            dir.x += dir.x > 0 ? 1 : -1;
        else if (dir.z != 0)
            dir.z += dir.z > 0 ? 1 : -1;

        // Disposes of the chunk row
        for (int row = -halfRenderDistance; row < halfRenderDistance + 1; row++)
        {
            if (dir.x == 0)
                Chunks[new(playerChunk.x + row, playerChunk.z + dir.z)].Dispose();
            else
                Chunks[new(playerChunk.x + dir.x, playerChunk.z + row)].Dispose();
        }
        
        
        // Updates the row in the direction of the player from the row that was disposed
        for (int row = -halfRenderDistance; row < halfRenderDistance + 1; row++)
        {
            if (dir.x == 0)
            {
                ChunkPosition pos = new(playerChunk.x + row, playerChunk.z + startDir.z);
                Chunks[pos].ChunkRenderer.RefreshBorderVoxels(Chunks[pos]);
            }
            else
            {
                ChunkPosition pos = new(playerChunk.x + startDir.x, playerChunk.z + row);
                Chunks[pos].ChunkRenderer.RefreshBorderVoxels(Chunks[pos]);
            }
        }
        
        
    }

    // Disposes of all chunks
    public static void Dispose(bool isEditor = false)
    {
        foreach (var chunk in Chunks)
            chunk.Value.Dispose(isEditor);
    }

    #region Editor
    public static void PreviewChunkEditor(ChunkPosition pos)
    {
        GenerateChunk(pos);
    }
    #endregion
}

public struct ChunkPosition
{
    public int x { get; set; }
    public int z { get; set; }

    public ChunkPosition(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static implicit operator Vector2Int(ChunkPosition v) => new Vector2Int(v.x, v.z);
    public static implicit operator ChunkPosition(Vector2Int v) => new ChunkPosition(v.x, v.y);
    public static implicit operator ChunkPosition((int, int) v) => new ChunkPosition(v.Item1, v.Item2);
    public static implicit operator (int, int)(ChunkPosition v) => (v.x, v.z);
    public static ChunkPosition operator +(ChunkPosition a, ChunkPosition b) => new ChunkPosition(a.x + b.x, a.z + b.z);
    public static ChunkPosition operator -(ChunkPosition a, ChunkPosition b) => new ChunkPosition(a.x - b.x, a.z - b.z);
    public static ChunkPosition operator *(ChunkPosition a, int b) => new ChunkPosition(a.x * b, a.z * b);
    public static ChunkPosition operator /(ChunkPosition a, int b) => new ChunkPosition(a.x / b, a.z / b);
    public static ChunkPosition operator -(ChunkPosition a) => new ChunkPosition(a.x * -1, a.z * -1);
    public static bool operator ==(ChunkPosition a, ChunkPosition b) => a.x == b.x && a.z == b.z;
    public static bool operator !=(ChunkPosition a, ChunkPosition b) => a.x != b.x || a.z != b.z;
    public override bool Equals(object obj) => obj is ChunkPosition v && v == this;
    public override int GetHashCode() => x.GetHashCode() ^ z.GetHashCode();
    public override string ToString() => $"({x}, {z})";

    public static ChunkPosition Zero => new ChunkPosition(0, 0);
    public static ChunkPosition One => new ChunkPosition(1, 1);
    public static ChunkPosition Forward => new ChunkPosition(0, 1);
    public static ChunkPosition Back => new ChunkPosition(0, -1);
    public static ChunkPosition Right => new ChunkPosition(1, 0);
    public static ChunkPosition Left => new ChunkPosition(-1, 0);
}