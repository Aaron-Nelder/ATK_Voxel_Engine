using System.Collections.Generic;

namespace ATKVoxelEngine
{
    public static class ChunkLoadManager 
    {
        const int CHUNK_LOAD_LIMIT = 1;

        static Queue<ChunkPosition> _chunksToLoad = new Queue<ChunkPosition>(); // The queue of chunks that are waiting to be loaded
        static Stack<ChunkPosition> _chunksLoading = new Stack<ChunkPosition>();// The stack of chunks that are actively loading

        // Adds a chunk to the load queue
        public static void QueueChunkForLoad(ChunkPosition pos, bool useThreads = true)
        {
            // checks to see if the chunk is already in the queue or is already loading
            if (_chunksToLoad.Contains(pos) || _chunksLoading.Contains(pos)) return;

            _chunksToLoad.Enqueue(pos);
            CheckForChunkLoad();
        }

        // Gets called when a chunk has finished loading
        public static void OnChunkLoaded(ChunkPosition pos)
        {
            if (_chunksLoading.Count <= 0) return;
            _chunksLoading.Pop();
            CheckForChunkLoad();
        }

        // Checks to see if there are any chunks to load
        static void CheckForChunkLoad()
        {
            if (_chunksToLoad.Count > 0 && _chunksLoading.Count <= CHUNK_LOAD_LIMIT)
            {
                _chunksLoading.Push(_chunksToLoad.Dequeue());
                ChunkManager.GenerateChunk(_chunksLoading.Peek());
            }
            else if (_chunksToLoad.Count <= 0)
                EngineManager.SetGameState(GameState.PLAYING);
        }
    }
}
