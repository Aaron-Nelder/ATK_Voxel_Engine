using System;
using UnityEngine;
using Unity.Mathematics;

namespace ATKVoxelEngine
{
    public static class WorldHelper
    {
        #region World to Local / Chunk

        // Returns the chunk position of the voxel position
        public static Chunk WorldPosToChunk(int3 wPos)
        {
            ChunkPosition chunkPos = new(Mathf.FloorToInt((float)(wPos.x / (float)EngineSettings.WorldSettings.ChunkSize.x)), Mathf.FloorToInt((float)(wPos.z / (float)EngineSettings.WorldSettings.ChunkSize.z)));

            if (ChunkManager.Chunks.ContainsKey(chunkPos))
                return ChunkManager.Chunks[chunkPos];

            return null;
        }

        // Returns the chunk position of the world position
        public static ChunkPosition WorldToChunkPos(Vector3 wPos)
        {
            int x = Mathf.FloorToInt((float)(wPos.x / (float)EngineSettings.WorldSettings.ChunkSize.x));
            int z = Mathf.FloorToInt((float)(wPos.z / (float)EngineSettings.WorldSettings.ChunkSize.z));
            return new(x, z);
        }

        // Takes in a world position and returns the chunk, local position, and ID
        public static bool WorldPosToChunk(int3 wPos, out Chunk chunk, out int3 vLocalPos, out uint voxelId)
        {
            vLocalPos = WorldToLocalPos(wPos);
            chunk = WorldPosToChunk(wPos);

            if (chunk == null || !VoxelInBounds(vLocalPos.y, EngineSettings.WorldSettings.ChunkSize.y))
            {
                voxelId = 0;
                return false;
            }

            voxelId = chunk.GetVoxel(vLocalPos);
            return true;
        }

        // Returns the local position of the voxel
        public static int3 WorldToLocalPos(int3 pos)
        {
            pos.x %= EngineSettings.WorldSettings.ChunkSize.x;
            pos.z %= EngineSettings.WorldSettings.ChunkSize.z;

            if (pos.x < 0)
                pos.x += EngineSettings.WorldSettings.ChunkSize.x;
            if (pos.z < 0)
                pos.z += EngineSettings.WorldSettings.ChunkSize.z;
            return pos;
        }

        // returns the Id of the voxel at the given world position
        public static uint WorldPosToId(int3 wPos)
        {
            int3 lPos = WorldToLocalPos(wPos);

            Chunk c = WorldPosToChunk(wPos);
            if (c == null)
                return 0;

            return c.GetVoxel(lPos);
        }

        #endregion

        #region Local to World / Id

        // Returns the world position of the voxel in the given chunk position
        public static int3 LocalPosToWorldPos(ChunkPosition chunkPos, int3 localPos)
        {
            localPos.x = chunkPos.x * EngineSettings.WorldSettings.ChunkSize.x + localPos.x;
            localPos.z = chunkPos.z * EngineSettings.WorldSettings.ChunkSize.z + localPos.z;
            return localPos;
        }

        // Returns the Id of the voxel at the given local position
        public static uint LocalPosToId(ChunkPosition chunkPos, int3 vPos)
        {
            int chunkSizeX = EngineSettings.WorldSettings.ChunkSize.x;
            int chunkSizeZ = EngineSettings.WorldSettings.ChunkSize.z;

            // if the voxel position is not in the chunk increase the chunk position
            if (vPos.x < 0)
            {
                chunkPos.x--;
                vPos.x += chunkSizeX;
            }
            else if (vPos.x >= chunkSizeX)
            {
                chunkPos.x++;
                vPos.x -= chunkSizeX;
            }
            if (vPos.z < 0)
            {
                chunkPos.z--;
                vPos.z += chunkSizeZ;
            }
            else if (vPos.z >= chunkSizeZ)
            {
                chunkPos.z++;
                vPos.z -= chunkSizeZ;
            }

            if (!VoxelInBounds(vPos.y, EngineSettings.WorldSettings.ChunkSize.y))
                return 0;

            if (!ChunkManager.Chunks.ContainsKey(chunkPos)) return 0;

            return ChunkManager.Chunks[chunkPos].GetVoxel(vPos);
        }

        // Returns the world position of the chunk
        public static int3 ChunkPosToWorldPos(ChunkPosition pos)
        {
            return new int3(Mathf.FloorToInt(pos.x * EngineSettings.WorldSettings.ChunkSize.x), 0, Mathf.FloorToInt(pos.z * EngineSettings.WorldSettings.ChunkSize.z));
        }

        // Returns true if the voxel position is occupied
        public static bool IsOccupied(ChunkPosition chunkPos, int3 position)
        {
            return LocalPosToId(chunkPos, position) != 0;
        }

        public static bool VoxelInBounds(int yPos, int chunkSizeY) => yPos >= 0 && yPos < chunkSizeY;

        public static int GetVoxelIndex(int3 pos, int3 chunkSize) => pos.x + pos.y * chunkSize.x + pos.z * chunkSize.x * chunkSize.y;

        public static int3 GetVoxelPos(int index, int3 chunkSize)
        {
            int x = index % chunkSize.x;
            int y = (index / chunkSize.x) % chunkSize.y;
            int z = index / (chunkSize.x * chunkSize.y);
            return new int3(x, y, z);
        }

        #endregion

        #region Casts

        // return true if a voxel is along the path of the ray, outs the closest voxel to the ray start point 
        public static bool VoxelCast(Ray ray, out SelectedVoxel selectedVoxel, float stepLength = 0.05f, float range = 5f)
        {
            selectedVoxel = SelectedVoxel.Empty;

            for (float i = 0; i < range; i += stepLength)
            {
                float3 currentPosition = (ray.direction.normalized * i) + ray.origin;

                int3 worldPosInt = new int3(math.round(currentPosition));

                if (WorldPosToChunk(worldPosInt, out Chunk chunk, out int3 lPos, out uint id))
                {
                    float3 nfloat = math.round(currentPosition - worldPosInt);
                    nfloat = math.normalize(nfloat);

                    int3 nInt = (int3)math.round(nfloat);
                    int xAbs = math.abs(nInt.x);
                    int yAbs = math.abs(nInt.y);
                    int zAbs = math.abs(nInt.z);

                    if (xAbs > yAbs && xAbs > zAbs)
                        nInt = new int3(nInt.x, 0, 0); // Align along X
                    else if (yAbs > xAbs && yAbs > zAbs)
                        nInt = new int3(0, nInt.y, 0); // Align along Y
                    else
                        nInt = new int3(0, 0, nInt.z); // Align along Z

                    if (!nInt.Equals(int3.zero))
                    {
                        selectedVoxel = new(chunk, lPos, id, worldPosInt + nInt);

                        if (id != 0)
                            return true;
                    }
                }
            }
            return false;
        }


        #endregion

        #region Directions
        // Returns the Vector3Int of all the directions
        // TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK
        public static readonly int3[] Directions = new int3[]
        {
        new int3(0,1,0),
        new int3(0,-1,0),
        new int3(-1,0,0),
        new int3(1,0,0),
        new int3(0,0,1),
        new int3(0,0,-1)
        };

        // Returns the Vector3Int of all the direction passsed in
        public static int3 FaceToVec(Face dirEnum)
        {
            switch (dirEnum)
            {
                case Face.TOP:
                    return new int3(0, 1, 0);
                case Face.BOTTOM:
                    return new int3(0, -1, 0);
                case Face.LEFT:
                    return new int3(-1, 0, 0);
                case Face.RIGHT:
                    return new int3(1, 0, 0);
                case Face.FRONT:
                    return new int3(0, 0, 1);
                case Face.BACK:
                    return new int3(0, 0, -1);
                default:
                    return int3.zero;
            }
        }

        public static int DirectionToInt(float3 dir)
        {
            switch (dir)
            {
                case var d when d.Equals(Directions[0]):
                    return 0;
                case var d when d.Equals(Directions[1]):
                    return 1;
                case var d when d.Equals(Directions[2]):
                    return 2;
                case var d when d.Equals(Directions[3]):
                    return 3;
                case var d when d.Equals(Directions[4]):
                    return 4;
                case var d when d.Equals(Directions[5]):
                    return 5;
                default:
                    return -1;
            }
        }

        #endregion

        // Sets the given voxel to air and surounding voxels to display their proper faces
        public static void DestroyVoxel(int3 vWorldPos)
        {
            if (!WorldPosToChunk(vWorldPos, out Chunk chunk, out int3 vLocalPos, out uint voxelId)) return;
            int3 chunkSize = EngineSettings.WorldSettings.ChunkSize;

            if (!chunk.Initialized) return;

            if (!VoxelInBounds(vLocalPos.y, chunkSize.y)) return;

            // sets the current voxel to air at the given world position
            chunk.SetVoxel(vLocalPos, 0);
            chunk.RenderManager.RemoveVoxel(vLocalPos);

            // loops through all directions and adds the new visible faces to the surrounding voxels
            foreach (var dir in Directions)
            {
                int3 checkPos = vLocalPos + dir;
                ChunkPosition chunkPos = chunk.Position;

                // If the voxel position is not in the chunk continue
                if (checkPos.x < 0) continue;
                else if (checkPos.x >= chunkSize.x) continue;

                if (checkPos.z < 0) continue;
                else if (checkPos.z >= chunkSize.z) continue;

                if (VoxelInBounds(checkPos.y, chunkSize.y))
                {
                    Chunk c = ChunkManager.Chunks[chunkPos];
                    c.RenderManager.AddVoxelFace(checkPos, -dir);
                }
            }
          
            chunk.RenderManager.SetMeshBuffer();
        }

        public static void PlaceVoxel(int3 vWPos, uint id)
        {
            int3 chunkSize = EngineSettings.WorldSettings.ChunkSize;
            if (!VoxelInBounds(vWPos.y, chunkSize.y)) return;   // Checks to make sure we're not trying to break a voxel that is out of bounds

            if (!WorldPosToChunk(vWPos, out Chunk chunk, out int3 vLPos, out uint voxelId)) return;

            chunk.SetVoxel(vLPos, id);

            // loops through all directions and adds the new visible faces to the surrounding voxels
            foreach (var dir in Directions)
            {
                int3 checkPos = vLPos + dir;

                // If the voxel position is not in the chunk continue
                if (checkPos.x < 0) continue;
                else if (checkPos.x >= chunkSize.x) continue;

                if (checkPos.z < 0) continue;
                else if (checkPos.z >= chunkSize.z) continue;

                if (VoxelInBounds(checkPos.y, chunkSize.y))
                {
                    if (chunk.GetVoxel(checkPos) == 0)
                        chunk.RenderManager.AddVoxelFace(vLPos, dir);
                    else
                        chunk.RenderManager.OnRemoveVoxelFace(checkPos, -dir);
                }
            }

            chunk.RenderManager.SetMeshBuffer();
        }

        public static bool IsBitSet(int num, int bitPos) => (num & (1 << bitPos)) != 0;
    }

    // Just a simple struct to quickly represent a 3D Int Direction
    public enum Face : byte
    {
        NULL = 0x000,
        TOP = 0x001,
        BOTTOM = 0x002,
        LEFT = 0x003,
        RIGHT = 0x004,
        FRONT = 0x005,
        BACK = 0x006,
    }

    // A struct to represent a selected voxel
    public struct SelectedVoxel : IEquatable<SelectedVoxel>
    {
        public Chunk Chunk { get; private set; }
        public int3 LocalPosition { get; private set; }
        public uint Id { get; private set; }
        public int3 NormalWorldPos { get; private set; }

        public SelectedVoxel(Chunk chunk, int3 localPosition, uint id, int3 normal)
        {
            Chunk = chunk;
            LocalPosition = localPosition;
            Id = id;
            NormalWorldPos = normal;
        }

        public bool Equals(SelectedVoxel other) => LocalPosition.Equals(other.LocalPosition) && Id.Equals(other.Id) && NormalWorldPos.Equals(other.NormalWorldPos);
        public static SelectedVoxel Empty => new(null, int3.zero, 0, int3.zero);
    }
}