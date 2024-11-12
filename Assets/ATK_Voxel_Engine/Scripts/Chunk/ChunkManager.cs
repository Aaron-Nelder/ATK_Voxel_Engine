using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ATKVoxelEngine
{
    public static class ChunkManager
    {
        static Transform chunkParent;

        public static ConcurrentDictionary<ChunkPosition, Chunk> Chunks { get; private set; } = new ConcurrentDictionary<ChunkPosition, Chunk>();

        public static Action<ChunkPosition, ChunkPosition> OnPlayerChunkUpdate;

        static ChunkPosition _lastKnownPlayerChunk = new ChunkPosition(0, 0);

        public static void SpawnStartingChunks(Transform parent)
        {
            chunkParent = parent;
            Dispose();

            GenerateChunksAtOrigin();

            TickRateManager.OnChunkLoadTick += OnChunkLoadTick;
            Application.quitting += () => TickRateManager.OnChunkLoadTick -= OnChunkLoadTick;
        }

        // Checks to see if the player hass entered a new chunk
        static void OnChunkLoadTick(float deltaTime)
        {
            ChunkPosition currentChunk = PlayerHelper.PlayerChunk;

            if (_lastKnownPlayerChunk == currentChunk) return;

            ChunkPosition dir = currentChunk - _lastKnownPlayerChunk;
            _lastKnownPlayerChunk = currentChunk;

            RefreshRenderDistance(currentChunk);
            //UnloadRenderDistance(currentChunk, dir);
            //LoadRenderDistance(currentChunk, dir);
        }

        static void RefreshRenderDistance(ChunkPosition chunkPos)
        {
            ushort halfRendDis = (ushort)(EngineSettings.WorldSettings.RenderDistance / 2);

            // unload all chunks that aren't within the render distance
            foreach (var chunk in Chunks)
                if (math.abs(chunk.Key.x - chunkPos.x) > halfRendDis || math.abs(chunk.Key.z - chunkPos.z) > halfRendDis)
                    chunk.Value.Dispose();

            // load all chunks that are within the render distance
            for (int x = -halfRendDis; x < halfRendDis + 1; x++)
                for (int z = -halfRendDis; z < halfRendDis + 1; z++)
                    if (!Chunks.ContainsKey(new(chunkPos.x + x, chunkPos.z + z)))
                        ChunkLoadManager.QueueChunkForLoad(new(chunkPos.x + x, chunkPos.z + z));
        }

        // Generates the starting chunks for the game
        static void GenerateChunksAtOrigin()
        {
            int interval = EngineSettings.WorldSettings.RenderDistance / 2;
            for (int x = -interval; x < interval + 1; x += 1)
                for (int z = -interval; z < interval + 1; z += 1)
                    ChunkLoadManager.QueueChunkForLoad(new(x, z), true);
        }

        // Get's a chunk prefab from the pool and starts loading it
        public static void GenerateChunk(ChunkPosition pos, bool useThreads = true)
        {
            if (Chunks.ContainsKey(pos)) return;

            GameObject chunkObj = EngineManager.Instance.Pool.ChunkOBJPool.Get();
            chunkObj.transform.position = EngineUtilities.ChunkPosToWorldPosVec3(pos);
            chunkObj.transform.SetParent(chunkParent);
            chunkObj.name = $"Chunk: ({pos.x},{pos.z})";
            chunkObj.GetComponent<Chunk>().Startup(pos, useThreads);
        }

        // Disposes of all chunks
        public static void Dispose()
        {
            foreach (var chunk in Chunks)
                chunk.Value.Dispose();
            Chunks.Clear();
        }

        #region Editor
        public static void PreviewChunkEditor(ChunkPosition pos) => GenerateChunk(pos, false);
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
}