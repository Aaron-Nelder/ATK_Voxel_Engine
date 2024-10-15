using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine.UIElements;

public struct ChunkRenderer
{
    //NativeHashMap<Vector3Int, NativeList<Vector3>> vertices;
    //NativeHashMap<Vector3Int, NativeList<int>> indices;
    //NativeHashMap<Vector3Int, NativeList<Vector2>> uvs;
    //NativeHashMap<Vector3Int, NativeList<Vector3>> normals;

    Dictionary<Vector3Int, List<Vector3>> vertices;
    Dictionary<Vector3Int, List<int>> indices;
    Dictionary<Vector3Int, List<Vector2>> uvs;
    Dictionary<Vector3Int, List<Vector3>> normals;

    // Initalizes the renderer with the given chunk
    public ChunkRenderer(Chunk chunk)
    {
        vertices = new Dictionary<Vector3Int, List<Vector3>>();
        indices = new Dictionary<Vector3Int, List<int>>();
        uvs = new Dictionary<Vector3Int, List<Vector2>>();
        normals = new Dictionary<Vector3Int, List<Vector3>>();
        InitalizeDictionary(chunk);
        RefreshVisibleVoxels(chunk);
    }

    // Fills the dictionary with the voxels
    void InitalizeDictionary(Chunk chunk)
    {
        foreach (var v in chunk.Voxels)
        {
            vertices.Add(v.Key, new());
            indices.Add(v.Key, new());
            uvs.Add(v.Key, new());
            normals.Add(v.Key, new());
        }
    }

    // Re-Generates the mesh for the chunk with only the blocks that were visible before
    public void RefreshVisibleVoxels(Chunk chunk)
    {
        foreach (var v in chunk.Voxels)
            RefreshVoxel(chunk, v.Key);
    }

    //Write me a function that only refreshes the border voxels
    public void RefreshBorderVoxels(Chunk chunk)
    {
        foreach (var v in chunk.Voxels)
        {
            if (v.Key.x == 0 || v.Key.x == VoxelManager.WorldSettings.chunkSize - 1 || v.Key.z == 0 || v.Key.z == VoxelManager.WorldSettings.chunkSize - 1)
            {
                RefreshVoxel(chunk, v.Key);
            }
        }
    }

    // Applies the combined vertices, indices, uvs and normals to one signal mesh and applies it to the mesh filter and collider
    public bool ApplyData(MeshFilter filter, MeshCollider collider)
    {
        List<Vector3> vertValues = new List<Vector3>();
        foreach (var b in vertices)
            vertValues.AddRange(b.Value);

        int vertOffset = 0;
        List<int> indicesValues = new List<int>();
        foreach (var b in indices)
        {
            foreach (var i in b.Value)
                indicesValues.Add(i + vertOffset);
            vertOffset += b.Value.Count;
        }

        List<Vector2> uvValues = new List<Vector2>();
        foreach (var b in uvs)
            uvValues.AddRange(b.Value);

        List<Vector3> normValues = new List<Vector3>();
        foreach (var b in normals)
            normValues.AddRange(b.Value);

        filter.sharedMesh.Clear();
        filter.sharedMesh.SetVertices(vertValues);
        filter.sharedMesh.SetIndices(indicesValues, MeshTopology.Quads, 0);
        filter.sharedMesh.SetUVs(0, uvValues);
        filter.sharedMesh.SetNormals(normValues);
        filter.sharedMesh.RecalculateBounds();

        collider.sharedMesh = filter.sharedMesh;

        return true;
    }

    public void RefreshVoxel(Chunk chunk, Vector3Int position, bool refreshMesh = false)
    {
        if (chunk.Voxels[position] == 0) return;
        VoxelData_SO data = VoxelManager.GetVoxelData(chunk.Voxels[position]);
        data.MeshData.GetVisibles(chunk.Position, position, out List<Vector3> vertices, out List<int> indices, out List<Vector2> uvs, out List<Vector3> normals);
        AddData(chunk, position, vertices, indices, data.ScaledUVs(uvs), normals);
        if (refreshMesh)
            ApplyData(chunk.Filter, chunk.Collider);
    }

    public void AddVoxelFace(Chunk chunk, Vector3Int position, Vector3Int faceNormal)
    {
        uint id = chunk.Voxels[position];
        if (id == 0) return;
        VoxelData_SO data = VoxelManager.GetVoxelData(id);
        data.MeshData.GetPlane(position, faceNormal, out List<Vector3> vertices, out List<int> indices, out List<Vector2> uvs, out List<Vector3> normals);

        AddData(chunk, position, vertices, indices, data.ScaledUVs(uvs), normals);
    }

    // Removes the given block from the mesh data
    public void RemoveData(Chunk chunk, Vector3Int pos)
    {
        vertices[pos].Clear();
        indices[pos].Clear();
        uvs[pos].Clear();
        normals[pos].Clear();
        chunk.MarkDirty();
    }

    void AddData(Chunk chunk, Vector3Int pos, List<Vector3> vertices, List<int> indices, List<Vector2> uvs, List<Vector3> normals)
    {
        this.vertices[pos].AddRange(vertices);
        foreach (var i in indices)
            this.indices[pos].Add(i + this.vertices[pos].Count - vertices.Count);
        this.uvs[pos].AddRange(uvs);
        this.normals[pos].AddRange(normals);
        chunk.MarkDirty();
    }
}
