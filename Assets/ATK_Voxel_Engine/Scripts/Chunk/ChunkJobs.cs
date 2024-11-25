using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace ATKVoxelEngine
{
    [BurstCompile]
    public struct AssignVoxelsJob : IJob
    {
        NativeArray<VoxelType> _voxels;
        NativeArray<int> _hNoiseMap;
        NativeArray<int> _cNoiseMap;
        readonly int3 _chunkSize;

        public AssignVoxelsJob(NativeArray<VoxelType> voxels, int3 chunkSize, NativeArray<int> hNoiseMap, NativeArray<int> cNoiseMap)
        {
            _voxels = voxels;
            _chunkSize = chunkSize;
            _hNoiseMap = hNoiseMap;
            _cNoiseMap = cNoiseMap;
        }

        public void Execute()
        {
            AssignVoxels();
        }

        void AssignVoxels()
        {
            // loop through the x and z axis
            for (int x = 0; x < _chunkSize.x; x++)
            {
                for (int z = 0; z < _chunkSize.z; z++)
                {
                    int height = _hNoiseMap[x + (z * _chunkSize.x)];
                    // loop through the y axis
                    for (int y = height; y >= 0; y--)
                    {
                        int index3D = PosToIndex3D(x, y, z);
                        if (index3D >= 0 && index3D < _voxels.Length)
                            _voxels[index3D] = SetVoxel(new(x, y, z), height);
                    }
                }
            }
        }

        VoxelType SetVoxel(int3 pos, int surfaceHeight)
        {
            bool isVoxel = _cNoiseMap[PosToIndex3D(pos.x, pos.y, pos.z)] == 0;
            //isVoxel = true;

            // BEDROCK
            if (pos.y == 0)
                return VoxelType.STONE;

            if (pos.y == surfaceHeight && isVoxel)
                return VoxelType.GRASS;

            // Top Soil
            else if (surfaceHeight - pos.y < 3 && isVoxel)
                return VoxelType.DIRT;

            if (surfaceHeight == 0)
                return VoxelType.STONE;

            // Underground
            return (isVoxel ? VoxelType.STONE : VoxelType.AIR);
        }

        int PosToIndex3D(int x, int y, int z) => x + y * _chunkSize.x + z * _chunkSize.x * _chunkSize.y;
    }

    [BurstCompile]
    public struct VisibleVoxelsJob : IJob
    {
        NativeArray<VoxelType> _voxels;
        NativeArray<int> _results;
        int3 _chunkSize;

        public VisibleVoxelsJob(NativeArray<VoxelType> voxelIds, NativeArray<int> resuts, int3 chunkSize)
        {
            _voxels = voxelIds;
            _results = resuts;
            _chunkSize = chunkSize;
        }

        public void Execute()
        {
            for (int i = 0; i < _results.Length; i++)
            {
                // returns 0 if the voxel is air
                if (_voxels[i] == 0)
                {
                    _results[i] = 0;
                    continue;
                }

                // Get the 3D position of the voxel
                int3 vPosInChunk = new int3(i % _chunkSize.x, (i / _chunkSize.x) % _chunkSize.y, i / (_chunkSize.x * _chunkSize.y));
                int visibleSides = 0;
                for (int j = 0; j < EngineUtilities.Directions.Length; j++)
                    SetVisibility(ref visibleSides, vPosInChunk + EngineUtilities.Directions[j], j);

                // Write the result to the output buffer
                _results[i] = visibleSides;
            }
        }

        // sets a bit in an int to 1
        void SetBit(ref int num, int pos) => num = num | (1 << pos);

        // checks if a position is within the bounds of the chunk
        bool InBounds(int3 pos)
        {
            return pos.x >= 0 && pos.x < _chunkSize.x &&
                   pos.y >= 0 && pos.y < _chunkSize.y &&
                   pos.z >= 0 && pos.z < _chunkSize.z;
        }

        void SetVisibility(ref int visInt, int3 voxelPos, int bitPos)
        {
            // if the voxel is in bounds, check if it is air, if it is, set the bit
            if (InBounds(voxelPos))
            {
                int index = voxelPos.x + voxelPos.y * _chunkSize.x + voxelPos.z * _chunkSize.x * _chunkSize.y;
                if (_voxels[index] == 0)
                    SetBit(ref visInt, bitPos);
            }

            // if the voxel is out of bounds, set the bit
            else
                SetBit(ref visInt, bitPos);
        }
    }

    public struct ColliderBakeJob : IJob
    {
        readonly int _meshId;

        public ColliderBakeJob(int meshIds)
        {
            _meshId = meshIds;
        }

        public void Execute()
        {
            UnityEngine.Physics.BakeMesh(_meshId, false);
        }
    }

    struct GenerateFolliageJob : IJob
    {
        NativeArray<Folliage> _folliages;
        NativeArray<int> _surfaceHeight;
        NativeArray<int> _folliageNoise;
        NativeArray<VoxelType> _voxels;
        int3 _chunkSize;
        uint _seed;
        ChunkPosition _chunkPos;

        public GenerateFolliageJob(uint seed, ChunkPosition chunkPos, NativeArray<Folliage> folliages, NativeArray<VoxelType> voxels, NativeArray<int> surfaceHeight, NativeArray<int> folliageNoise, int3 chunkSize)
        {
            _seed = seed;
            _folliages = folliages;
            _voxels = voxels;
            _surfaceHeight = surfaceHeight;
            _folliageNoise = folliageNoise;
            _chunkSize = chunkSize;
            _chunkPos = chunkPos;
        }

        const int MAX_TRIES = 25;
        public void Execute()
        {
            Random rng = new Random(_seed + (uint)math.abs((_chunkPos.x - _chunkPos.z)));
            int tries = 0;
            int placed = 0;

            // Loops though all types of folliage
            for (int i = 0; i < _folliages.Length; i++)
            {
                // TODO:: Don't rely on class
                FolliageData_SO folliageData = EngineSettings.GetFolliageData(_folliages[i].type);               

                for (int j = 0; j < MAX_TRIES; j++)
                {
                    if (placed >= (uint)_folliages[i].density)
                        break;

                    // Gets a random x and z position within the chunk
                    int3 surfacePos;
                    surfacePos.x = rng.NextInt(0, _chunkSize.x);
                    surfacePos.z = rng.NextInt(0, _chunkSize.z);

                    // Checks the noise map for if folliage is allowed to be placed at the position
                    if (_folliageNoise[surfacePos.x + (surfacePos.z * _chunkSize.x)] != 1)
                    {
                        tries++;
                        continue;
                    }

                    surfacePos.y = _surfaceHeight[surfacePos.x + (surfacePos.z * _chunkSize.x)];

                    VoxelType surfaceVoxel = _voxels[surfacePos.x + surfacePos.y * _chunkSize.x + surfacePos.z * _chunkSize.x * _chunkSize.y];

                    bool isPlaceable = false;

                    // Loops through all the placeable voxels for the folliage
                    for (int k = 0; k < folliageData.PlaceableVoxels.Length; k++)
                    {
                        if (surfaceVoxel == folliageData.PlaceableVoxels[k])
                        {
                            isPlaceable = true;
                            break;
                        }
                    }

                    if (!isPlaceable)
                    {
                        tries++;
                        continue;
                    }

                    // If the voxel at the surface position is a placeable voxel for the folliage

                    bool inBounds = true;
                    VoxelStructure structure = folliageData.GetRandomFolliage(ref rng);
                    for (int k = 0; k < structure.Positions.Length; k++)
                    {
                        int3 voxelPos = structure.Positions[k] + surfacePos + new int3(0, 1, 0);
                        if (!IsVoxelPosInChunk(voxelPos, _chunkSize) || _voxels[voxelPos.x + voxelPos.y * _chunkSize.x + voxelPos.z * _chunkSize.x * _chunkSize.y] != VoxelType.AIR)
                        {
                            inBounds = false;
                            break;
                        }
                    }

                    if (!inBounds)
                    {
                        tries++;
                        continue;
                    }

                    for (int k = 0; k < structure.Positions.Length; k++)
                    {
                        int3 voxelPos = structure.Positions[k] + surfacePos + new int3(0, 1, 0);
                        _voxels[voxelPos.x + voxelPos.y * _chunkSize.x + voxelPos.z * _chunkSize.x * _chunkSize.y] = structure.Ids[k];
                    }
                }
                tries++;
            }
        }

        bool IsVoxelPosInChunk(int3 pos, int3 chunkSize)
        {
            if (pos.x < 0 || pos.x >= chunkSize.x) return false;
            if (pos.y <= 0 || pos.y >= chunkSize.y - 1) return false;
            if (pos.z < 0 || pos.z >= chunkSize.z) return false;
            return true;
        }
    }
}