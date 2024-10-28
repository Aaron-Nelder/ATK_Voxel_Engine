using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    public static class WorldHelper
    {
        #region World to Local / Chunk

        // Returns the chunk position of the voxel position
        public static Chunk WorldPosToChunk(Vector3Int worldPosition)
        {
            ChunkPosition chunkPos = new(Mathf.FloorToInt((float)(worldPosition.x / (float)EngineSettings.WorldSettings.ChunkSize.x)), Mathf.FloorToInt((float)(worldPosition.z / (float)EngineSettings.WorldSettings.ChunkSize.z)));

            if (ChunkManager.Chunks.ContainsKey(chunkPos))
                return ChunkManager.Chunks[chunkPos];

            return null;
        }

        // Takes in a world position and returns the chunk, local position, and ID
        public static bool WorldPosToChunk(Vector3Int worldPosition, out Chunk chunk, out Vector3Int voxelLocalPos, out uint voxelId)
        {
            voxelLocalPos = WorldToLocalPos(worldPosition);
            chunk = WorldPosToChunk(worldPosition);

            if (chunk == null)
            {
                voxelId = 0;
                return false;
            }

            voxelId = chunk.Voxels.GetValueOrDefault(voxelLocalPos);

            return true;
        }

        // Returns the local position of the voxel
        public static Vector3Int WorldToLocalPos(Vector3Int pos)
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
        public static uint WorldPosToId(Vector3Int worldPosition)
        {
            Vector3Int localPos = WorldToLocalPos(worldPosition);

            Chunk c = WorldPosToChunk(worldPosition);
            if (c == null)
                return 0;

            return c.Voxels.GetValueOrDefault(localPos);
        }

        #endregion

        #region Local to World / Id

        // Returns the world position of the voxel in the given chunk position
        public static Vector3Int LocalPosToWorldPos(ChunkPosition chunkPos, Vector3Int localPos)
        {
            localPos.x = chunkPos.x * EngineSettings.WorldSettings.ChunkSize.x + localPos.x;
            localPos.z = chunkPos.z * EngineSettings.WorldSettings.ChunkSize.z + localPos.z;
            return localPos;
        }

        // Returns the Id of the voxel at the given local position
        public static uint LocalPosToId(ChunkPosition chunkPos, Vector3Int voxelPos)
        {
            int chunkSizeX = EngineSettings.WorldSettings.ChunkSize.x;
            int chunkSizeZ = EngineSettings.WorldSettings.ChunkSize.z;

            // if the voxel position is not in the chunk increase the chunk position
            if (voxelPos.x < 0)
            {
                chunkPos.x--;
                voxelPos.x += chunkSizeX;
            }
            else if (voxelPos.x >= chunkSizeX)
            {
                chunkPos.x++;
                voxelPos.x -= chunkSizeX;
            }
            if (voxelPos.z < 0)
            {
                chunkPos.z--;
                voxelPos.z += chunkSizeZ;
            }
            else if (voxelPos.z >= chunkSizeZ)
            {
                chunkPos.z++;
                voxelPos.z -= chunkSizeZ;
            }

            if (!ChunkManager.Chunks.ContainsKey(chunkPos)) return 0;

            return ChunkManager.Chunks[chunkPos].Voxels.GetValueOrDefault(voxelPos);
        }

        // Returns the world position of the chunk
        public static Vector3Int ChunkPosToWorldPos(ChunkPosition pos)
        {
            return new Vector3Int(Mathf.FloorToInt(pos.x * EngineSettings.WorldSettings.ChunkSize.x), 0, Mathf.FloorToInt(pos.z * EngineSettings.WorldSettings.ChunkSize.z));
        }

        // Returns true if the voxel position is occupied
        public static bool IsOccupied(ChunkPosition chunkPos, Vector3Int position)
        {
            return LocalPosToId(chunkPos, position) != 0;
        }

        public static bool VoxelInBounds(int yPos, int chunkSizeY)
        {
            return yPos > 0 || yPos <= chunkSizeY;
        }

        #endregion

        #region Casts

        // return true if a voxel is along the path of the ray, outs the closest voxel to the ray start point 
        public static bool VoxelCast(Ray ray, out SelectedVoxel selectedVoxel, float stepLength = 0.05f, float range = 5f)
        {
            selectedVoxel = SelectedVoxel.Empty;

            for (float i = 0; i < range; i += stepLength)
            {
                Vector3 currentPosition = (ray.direction.normalized * i) + ray.origin;
                Vector3Int worldPosInt = Vector3Int.RoundToInt(currentPosition);

                if (WorldPosToChunk(worldPosInt, out Chunk chunk, out Vector3Int localPosition, out uint id))
                {
                    Vector3Int norm = Vector3Int.RoundToInt((currentPosition - worldPosInt).normalized);
                    int xAbs = Math.Abs(norm.x);
                    int yAbs = Math.Abs(norm.y);
                    int zAbs = Math.Abs(norm.z);

                    if (xAbs > yAbs && xAbs > zAbs)
                        norm = new Vector3Int(norm.x, 0, 0); // Align along X
                    else if (yAbs > xAbs && yAbs > zAbs)
                        norm = new Vector3Int(0, norm.y, 0); // Align along Y
                    else
                        norm = new Vector3Int(0, 0, norm.z); // Align along Z

                    if (norm != Vector3Int.zero)
                    {
                        selectedVoxel = new(chunk, localPosition, id, worldPosInt + norm);

                        if (id != 0)
                            return true;
                    }
                }
            }
            return false;
        }


        #endregion

        #region Directions
        // Returns the Vector3Int of all the directionss
        public static Vector3Int[] Directions = new Vector3Int[]
        {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.forward,
        Vector3Int.back
        };

        // Returns the Vector3Int of all the direction passsed in
        public static Vector3Int FaceToVec(Face dirEnum)
        {
            switch (dirEnum)
            {
                case Face.TOP:
                    return Vector3Int.up;
                case Face.BOTTOM:
                    return Vector3Int.down;
                case Face.LEFT:
                    return Vector3Int.left;
                case Face.RIGHT:
                    return Vector3Int.right;
                case Face.FRONT:
                    return Vector3Int.forward;
                case Face.BACK:
                    return Vector3Int.back;
                default:
                    return Vector3Int.zero;
            }
        }

        // Returns the DirectionEnum of the Vector3Int passed in
        public static Face VectorToDirectionEnum(Vector3Int dir)
        {
            if (dir == Vector3Int.up)
                return Face.TOP;
            if (dir == Vector3Int.down)
                return Face.BOTTOM;
            if (dir == Vector3Int.left)
                return Face.LEFT;
            if (dir == Vector3Int.right)
                return Face.RIGHT;
            if (dir == Vector3Int.forward)
                return Face.FRONT;
            if (dir == Vector3Int.back)
                return Face.BACK;

            return Face.NULL;
        }

        #endregion

        // Sets the given voxel to air and surounding voxels to display their proper faces
        public static void DestroyVoxel(Vector3Int voxelWorldPosition)
        {
            if(!WorldPosToChunk(voxelWorldPosition, out Chunk chunk, out Vector3Int voxelLocalPos, out uint voxelId)) return;
            Vector3Int chunkSize = EngineSettings.WorldSettings.ChunkSize;

            if (!chunk.Initialized) return;

            if (!VoxelInBounds(voxelLocalPos.y, chunkSize.y)) return;

            // sets the current voxel to air at the given world position
            chunk.ChunkRenderer.ClearData(chunk, voxelLocalPos);
            chunk.Voxels[voxelLocalPos] = 0;

            // loops through all directions and adds the new visible faces to the surrounding voxels
            foreach (var dir in Directions)
            {
                Vector3Int checkPos = voxelLocalPos + dir;
                ChunkPosition chunkPos = chunk.Position;

                // If the voxel position is not in the chunk increase the chunk position
                if (checkPos.x < 0)
                {
                    chunkPos.x--;
                    checkPos.x += chunkSize.x;
                }
                else if (checkPos.x >= chunkSize.x)
                {
                    chunkPos.x++;
                    checkPos.x -= chunkSize.x;
                }

                if (checkPos.z < 0)
                {
                    chunkPos.z--;
                    checkPos.z += chunkSize.z;
                }
                else if (checkPos.z >= chunkSize.z)
                {
                    chunkPos.z++;
                    checkPos.z -= chunkSize.z;
                }

                if (VoxelInBounds(checkPos.y, chunkSize.y))
                {
                    Chunk c = ChunkManager.Chunks[chunkPos];
                    c.ChunkRenderer.AddVoxelFace(c, checkPos, -dir);
                }
            }

            chunk.ChunkRenderer.ApplyData(chunk);
        }

        public static void PlaceVoxel(Vector3Int voxelWorldPosition, uint id)
        {
            if (!WorldPosToChunk(voxelWorldPosition, out Chunk chunk, out Vector3Int voxelLocalPos, out uint voxelId)) return;
            Vector3Int chunkSize = EngineSettings.WorldSettings.ChunkSize;

            if (!VoxelInBounds(voxelLocalPos.y, chunkSize.y)) return;

            chunk.Voxels[voxelLocalPos] = id;
            chunk.ChunkRenderer.AddVisibleFaces(chunk, voxelLocalPos);

            // loops through all directions and adds the new visible faces to the surrounding voxels
            foreach (var dir in Directions)
            {
                Vector3Int checkPos = voxelLocalPos + dir;
                ChunkPosition chunkPos = chunk.Position;

                // If the voxel position is not in the chunk increase the chunk position
                if (checkPos.x < 0)
                {
                    chunkPos.x--;
                    checkPos.x += chunkSize.x;
                }
                else if (checkPos.x >= chunkSize.x)
                {
                    chunkPos.x++;
                    checkPos.x -= chunkSize.x;
                }

                if (checkPos.z < 0)
                {
                    chunkPos.z--;
                    checkPos.z += chunkSize.z;
                }
                else if (checkPos.z >= chunkSize.z)
                {
                    chunkPos.z++;
                    checkPos.z -= chunkSize.z;
                }

                if (VoxelInBounds(checkPos.y, chunkSize.y))
                    ChunkManager.Chunks[chunkPos].ChunkRenderer.AddVisibleFaces(ChunkManager.Chunks[chunkPos], checkPos);
            }

            chunk.ChunkRenderer.ApplyData(chunk);
        }
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
        public Vector3Int LocalPosition { get; private set; }
        public uint Id { get; private set; }
        public Vector3Int NormalWorldPos { get; private set; }

        public SelectedVoxel(Chunk chunk, Vector3Int localPosition, uint id, Vector3Int normal)
        {
            Chunk = chunk;
            LocalPosition = localPosition;
            Id = id;
            NormalWorldPos = normal;
        }

        public bool Equals(SelectedVoxel other)
        {
            return LocalPosition == other.LocalPosition && Id == other.Id && other.NormalWorldPos == NormalWorldPos;
        }

        public static SelectedVoxel Empty => new(null, Vector3Int.zero, 0, Vector3Int.zero);
    }
}