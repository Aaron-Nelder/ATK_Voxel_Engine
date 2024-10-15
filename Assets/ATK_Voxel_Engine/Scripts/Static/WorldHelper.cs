using System;
using System.Collections.Generic;
using UnityEngine;

public static class WorldHelper
{
    #region World to Local / Chunk

    // Returns the chunk position of the voxel position
    public static Chunk WorldPosToChunk(Vector3Int worldPosition)
    {
        float chunkSize = VoxelManager.WorldSettings.chunkSize;
        ChunkPosition chunkPos = new(Mathf.FloorToInt((float)(worldPosition.x / chunkSize)), Mathf.FloorToInt((float)(worldPosition.z / chunkSize)));

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
        int chunkSize = VoxelManager.WorldSettings.chunkSize;
        pos.x %= chunkSize;
        pos.z %= chunkSize;

        if (pos.x < 0)
            pos.x += chunkSize;
        if (pos.z < 0)
            pos.z += chunkSize;
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
        int chunkSize = VoxelManager.WorldSettings.chunkSize;
        localPos.x = chunkPos.x * chunkSize + localPos.x;
        localPos.z = chunkPos.z * chunkSize + localPos.z;
        return localPos;
    }

    // Returns the Id of the voxel at the given local position
    public static uint LocalPosToId(ChunkPosition chunkPos, Vector3Int voxelPos)
    {
        // if the voxel position is not in the chunk increase the chunk position
        if (voxelPos.x < 0)
        {
            chunkPos.x--;
            voxelPos.x += VoxelManager.WorldSettings.chunkSize;
        }
        else if (voxelPos.x >= VoxelManager.WorldSettings.chunkSize)
        {
            chunkPos.x++;
            voxelPos.x -= VoxelManager.WorldSettings.chunkSize;
        }
        if (voxelPos.z < 0)
        {
            chunkPos.z--;
            voxelPos.z += VoxelManager.WorldSettings.chunkSize;
        }
        else if (voxelPos.z >= VoxelManager.WorldSettings.chunkSize)
        {
            chunkPos.z++;
            voxelPos.z -= VoxelManager.WorldSettings.chunkSize;
        }

        if (!ChunkManager.Chunks.ContainsKey(chunkPos)) return 0;

        return ChunkManager.Chunks[chunkPos].Voxels.GetValueOrDefault(voxelPos);
    }

    // Returns the world position of the chunk
    public static Vector3Int ChunkPosToWorldPos(ChunkPosition pos)
    {
        int chunkSize = VoxelManager.WorldSettings.chunkSize;
        return new Vector3Int(Mathf.FloorToInt(pos.x * chunkSize), 0, Mathf.FloorToInt(pos.z * chunkSize));
    }

    // Returns true if the voxel position is occupied
    public static bool IsOccupied(ChunkPosition chunkPos, Vector3Int position)
    {
        return LocalPosToId(chunkPos, position) != 0;
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

    // Returns the Vector3Int of all the directions
    public static Face[] DirectionsEnums = new Face[]
    {
        Face.Top,Face.Bottom, Face.Left, Face.Right, Face.Front, Face.Back
    };

    // Returns the Vector3Int of all the direction passsed in
    public static Vector3Int DirectionEnumToVector(Face dirEnum)
    {
        switch (dirEnum)
        {
            case Face.Top:
                return Vector3Int.up;
            case Face.Bottom:
                return Vector3Int.down;
            case Face.Left:
                return Vector3Int.left;
            case Face.Right:
                return Vector3Int.right;
            case Face.Front:
                return Vector3Int.forward;
            case Face.Back:
                return Vector3Int.back;
            default:
                return Vector3Int.zero;
        }
    }

    // Returns the DirectionEnum of the Vector3Int passed in
    public static Face VectorToDirectionEnum(Vector3Int dir)
    {
        if (dir == Vector3Int.up)
            return Face.Top;
        if (dir == Vector3Int.down)
            return Face.Bottom;
        if (dir == Vector3Int.left)
            return Face.Left;
        if (dir == Vector3Int.right)
            return Face.Right;
        if (dir == Vector3Int.forward)
            return Face.Front;
        if (dir == Vector3Int.back)
            return Face.Back;

        return Face.NULL;
    }

    #endregion
}

// Just a simple struct to quickly represent a 3D Int Direction
public enum Face
{
    Top = 0,
    Bottom = 1,
    Left = 2,
    Right = 3,
    Front = 4,
    Back = 5,
    NULL = 6,
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
