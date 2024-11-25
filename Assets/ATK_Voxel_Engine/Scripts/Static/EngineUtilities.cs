using System;
using UnityEngine;
using Unity.Mathematics;

namespace ATKVoxelEngine
{
    public static class EngineUtilities
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
        public static bool WorldPosToChunk(int3 wPos, out Chunk chunk, out int3 vLocalPos, out VoxelType voxelId)
        {
            vLocalPos = WorldToLocalPos(wPos);
            chunk = WorldPosToChunk(wPos);

            if (chunk == null || !IsVoxelPosInChunk(vLocalPos, EngineSettings.WorldSettings.ChunkSize))
            {
                voxelId = VoxelType.AIR;
                return false;
            }

            voxelId = chunk.Data.GetVoxel(vLocalPos);
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
        #endregion

        #region Local to World / Id

        // Returns the world position of the voxel in the given chunk position
        public static int3 LocalPosToWorldPos(ChunkPosition chunkPos, int3 localPos)
        {
            localPos.x = chunkPos.x * EngineSettings.WorldSettings.ChunkSize.x + localPos.x;
            localPos.z = chunkPos.z * EngineSettings.WorldSettings.ChunkSize.z + localPos.z;
            return localPos;
        }

        // Returns the world position of the chunk
        public static int3 ChunkPosToWorldPosInt3(ChunkPosition pos)
        {
            return new int3(Mathf.FloorToInt(pos.x * EngineSettings.WorldSettings.ChunkSize.x), 0, Mathf.FloorToInt(pos.z * EngineSettings.WorldSettings.ChunkSize.z));
        }

        // Returns the world position of the chunk
        public static Vector3 ChunkPosToWorldPosVec3(ChunkPosition pos)
        {
            return new Vector3(Mathf.FloorToInt(pos.x * EngineSettings.WorldSettings.ChunkSize.x), 0, Mathf.FloorToInt(pos.z * EngineSettings.WorldSettings.ChunkSize.z));
        }

        public static bool IsVoxelPosInChunk(int3 pos, int3 chunkSize)
        {
            if (pos.x < 0 || pos.x >= chunkSize.x) return false;
            if (pos.y < 0 || pos.y >= chunkSize.y - 1) return false;
            if (pos.z < 0 || pos.z >= chunkSize.z) return false;
            return true;
        }

        public static int3 GetVoxelPos(int index, int3 chunkSize)
        {
            int x = index % chunkSize.x;
            int y = (index / chunkSize.x) % chunkSize.y;
            int z = index / (chunkSize.x * chunkSize.y);
            return new int3(x, y, z);
        }

        #endregion

        #region Casts
        // return true if a voxel is along the path of the ray, outputs the closest voxel to the ray start point that isn't 0 
        public static bool VoxelCast(Ray ray, out VoxelCastHit selectedVoxel, float stepLength = 0.05f, float range = 5f)
        {
            selectedVoxel = VoxelCastHit.Empty;
            int3 lastEnteredPos = int3.zero;

            for (float i = 0; i < range; i += stepLength)
            {
                float3 stepPos = (ray.direction.normalized * i) + ray.origin;   // world position of the ray
                int3 wPos = new int3(math.round(stepPos));                      // worldPosition of the voxel
                if (wPos.Equals(lastEnteredPos)) continue;                      // continues if we're checking the same voxel

                // if the voxel is in bounds and the voxel is not air
                if (WorldPosToChunk(wPos, out Chunk chunk, out int3 lPos, out VoxelType id))
                {
                    float3 fNorm = (float3)(lastEnteredPos - wPos);     // calculates the normal of the face
                    fNorm = math.normalize(fNorm);
                    int3 iNorm = (int3)fNorm;
                    if (id != 0 && iNorm.Equals(int3.zero)) continue;   // check to make sure the normal isn't inside another voxel
                    selectedVoxel = new(chunk, lPos, id, iNorm);        // sets the selected voxel
                    if (id != 0) return true;                           // returns true if the voxel is not air
                }
                lastEnteredPos = wPos;
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

        public static readonly ChunkPosition[] ChunkDirections = {
            new ChunkPosition(0, 1),
            new ChunkPosition(0, -1),
            new ChunkPosition(1, 0),
            new ChunkPosition(-1, 0),
            new ChunkPosition(1, 1),
            new ChunkPosition(1, -1),
            new ChunkPosition(-1, 1),
            new ChunkPosition(-1, -1)
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
            if (!WorldPosToChunk(vWorldPos, out Chunk chunk, out int3 vLocalPos, out VoxelType voxelId)) return;
            if (chunk.Data.State != ChunkState.LOADED) return;

            int3 chunkSize = EngineSettings.WorldSettings.ChunkSize;

            // sets the current voxel to air at the given world position
            chunk.Data.SetVoxel(vLocalPos, 0);
            chunk.RenderManager.RemoveVoxel(vLocalPos);

            // loops through all directions and adds the new visible faces to the surrounding voxels
            foreach (var dir in Directions)
            {
                int3 checkPos = vLocalPos + dir;

                // If the voxel position is not in the chunk continue
                if (!IsVoxelPosInChunk(checkPos, chunkSize))
                    continue;
                else
                    chunk.RenderManager.AddVoxelFace(checkPos, -dir);
            }

            chunk.RenderManager.SetMeshBuffer();
        }

        public static void PlaceVoxel(int3 vWPos, VoxelType id)
        {
            // This is here to ensure we don't place a voxel in the same position as the player
            if (PlayerHelper.PlayerVoxelPosition.Equals(vWPos) || PlayerHelper.PlayerVoxelPosition.Equals(vWPos + new int3(0, 1, 0))) return;
            if (!WorldPosToChunk(vWPos, out Chunk chunk, out int3 vLPos, out VoxelType voxelId)) return;
            if (chunk.Data.State != ChunkState.LOADED) return;

            int3 chunkSize = EngineSettings.WorldSettings.ChunkSize;

            chunk.Data.SetVoxel(vLPos, id);

            // loops through all directions and adds the new visible faces to the surrounding voxels
            foreach (var dir in Directions)
            {
                int3 checkPos = vLPos + dir;

                // If the voxel position is not in the chunk continue
                if (!IsVoxelPosInChunk(checkPos, chunkSize))
                    chunk.RenderManager.AddVoxelFace(vLPos, dir);
                else
                {
                    if (chunk.Data.GetVoxel(checkPos) == VoxelType.AIR)
                        chunk.RenderManager.AddVoxelFace(vLPos, dir);
                    else
                        chunk.RenderManager.RemoveVoxelFace(checkPos, -dir);
                }
            }

            chunk.RenderManager.SetMeshBuffer();
        }

        public static bool IsChunkVisible(Bounds bounds)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(PlayerManager.Instance.PlayerCamera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }

        public static bool IsBitSet(int num, int bitPos) => (num & (1 << bitPos)) != 0;

        public static int PosToIndex3D(int3 chunkSize, int x, int y, int z) => x + y * chunkSize.x + z * chunkSize.x * chunkSize.y;
    }

    // A struct to represent a selected voxel
    public struct VoxelCastHit : IEquatable<VoxelCastHit>
    {
        public Chunk Chunk { get; private set; }
        public int3 LocalPosition { get; private set; }
        public VoxelType Id { get; private set; }
        public int3 Normal { get; private set; }

        public int3 VoxelWorldPos => Selector.SelectedVoxel.LocalPosition + Selector.SelectedVoxel.Chunk.Data.WorldPosition;
        public int3 NormalWorldPos => Selector.SelectedVoxel.LocalPosition + Selector.SelectedVoxel.Chunk.Data.WorldPosition + Selector.SelectedVoxel.Normal;

        public VoxelCastHit(Chunk chunk, int3 localPosition, VoxelType id, int3 normal)
        {
            Chunk = chunk;
            LocalPosition = localPosition;
            Id = id;
            Normal = normal;
        }

        public bool Equals(VoxelCastHit other) => LocalPosition.Equals(other.LocalPosition) && Id.Equals(other.Id) && Normal.Equals(other.Normal);
        public static VoxelCastHit Empty => new(null, int3.zero, 0, int3.zero);
    }

    // A struct that hold a custome Structure
    [Serializable]
    public struct VoxelStructure
    {
        [field: SerializeField] public int3[] Positions { get; private set; }
        [field: SerializeField] public VoxelType[] Ids { get; private set; }

        public VoxelStructure(int3[] positions, VoxelType[] ids)
        {
            Positions = positions;
            Ids = ids;
        }
    }
}