using UnityEngine;
using System.Collections.Generic;
using System;

namespace ATKVoxelEngine
{
    public struct ChunkRenderer
    {
        //NativeHashMap<Vector3Int, NativeList<Vector3>> vertices;
        //NativeHashMap<Vector3Int, NativeList<int>> indices;
        //NativeHashMap<Vector3Int, NativeList<Vector2>> uvs;
        //NativeHashMap<Vector3Int, NativeList<Vector3>> normals;

        Dictionary<Vector3Int, List<Vector3>> _vertices;
        Dictionary<Vector3Int, List<int>> _indices;
        Dictionary<Vector3Int, List<Vector2>> _uvs;
        Dictionary<Vector3Int, List<Vector3>> _normals;

        // Initalizes the renderer with the given chunk
        public ChunkRenderer(Chunk chunk)
        {
            _vertices = new Dictionary<Vector3Int, List<Vector3>>();
            _indices = new Dictionary<Vector3Int, List<int>>();
            _uvs = new Dictionary<Vector3Int, List<Vector2>>();
            _normals = new Dictionary<Vector3Int, List<Vector3>>();
            InitalizeDictionary(chunk);
            RefreshVisibleVoxels(chunk);
        }

        // Fills the dictionary with the voxels
        void InitalizeDictionary(Chunk chunk)
        {
            foreach (var v in chunk.Voxels)
            {
                _vertices.Add(v.Key, new());
                _indices.Add(v.Key, new());
                _uvs.Add(v.Key, new());
                _normals.Add(v.Key, new());
            }
        }

        // Re-Generates the mesh for the chunk with only the blocks that were visible before
        public void RefreshVisibleVoxels(Chunk chunk)
        {
            foreach (var v in chunk.Voxels)
                AddVisibleFaces(chunk, v.Key);
        }

        //Write me a function that only refreshes the border voxels
        public void RefreshBorderVoxels(Chunk chunk)
        {
            foreach (var v in chunk.Voxels)
            {
                if (v.Key.x == 0 || v.Key.x == VoxelManager.WorldSettings.ChunkSize.x - 1 || v.Key.z == 0 || v.Key.z == VoxelManager.WorldSettings.ChunkSize.z - 1)
                {
                    AddVisibleFaces(chunk, v.Key);
                }
            }
        }

        // Applies the combined vertices, indices, uvs and normals to one signal mesh and applies it to the mesh filter and collider
        public void ApplyData(MeshFilter filter, MeshCollider collider, Action callback = null)
        {
            List<Vector3> vertValues = new List<Vector3>();
            foreach (var b in _vertices)
                vertValues.AddRange(b.Value);

            int vertOffset = 0;
            List<int> indicesValues = new List<int>();
            foreach (var b in _indices)
            {
                foreach (var i in b.Value)
                    indicesValues.Add(i + vertOffset);
                vertOffset += b.Value.Count;
            }

            List<Vector2> uvValues = new List<Vector2>();
            foreach (var b in _uvs)
                uvValues.AddRange(b.Value);

            List<Vector3> normValues = new List<Vector3>();
            foreach (var b in _normals)
                normValues.AddRange(b.Value);

            filter.sharedMesh.Clear();
            filter.sharedMesh.SetVertices(vertValues);
            filter.sharedMesh.SetIndices(indicesValues, MeshTopology.Quads, 0);
            filter.sharedMesh.SetUVs(0, uvValues);
            filter.sharedMesh.SetNormals(normValues);
            filter.sharedMesh.RecalculateBounds();

            collider.sharedMesh = filter.sharedMesh;
            callback?.Invoke();
        }

        public void AddVisibleFaces(Chunk chunk, Vector3Int position)
        {
            if (chunk.Voxels[position] == 0) return;
            VoxelData_SO data = VoxelManager.GetVoxelData(chunk.Voxels[position]);

            if (!data.MeshData.GetVisiblesPlanes(chunk.Position, position, out List<Vector3> vertices, out List<int> indices, out List<Vector2> uvs, out List<Vector3> normals))
                return;

            SetData(chunk, position, vertices, indices, data.ScaledUVs(uvs), normals);
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
        public void ClearData(Chunk chunk, Vector3Int pos)
        {
            _vertices[pos].Clear();
            _indices[pos].Clear();
            _uvs[pos].Clear();
            _normals[pos].Clear();
            chunk.MarkDirty();
        }

        void SetData(Chunk chunk, Vector3Int pos, List<Vector3> vertices, List<int> indices, List<Vector2> uvs, List<Vector3> normals)
        {
            _vertices[pos] = vertices;

            List<int> indicesCopy = new List<int>(indices);
            for (int i = 0; i < indices.Count; i++)
                indicesCopy[i] = indicesCopy[i] + _vertices[pos].Count - vertices.Count;

            _indices[pos] = indicesCopy;
            _uvs[pos] = uvs;
            _normals[pos] = normals;
            chunk.MarkDirty();
        }

        void AddData(Chunk chunk, Vector3Int pos, List<Vector3> vertices, List<int> indices, List<Vector2> uvs, List<Vector3> normals)
        {
            _vertices[pos].AddRange(vertices);

            List<int> indicesCopy = new List<int>(indices);
            for (int i = 0; i < indices.Count; i++)
                indicesCopy[i] = indicesCopy[i] + _vertices[pos].Count - vertices.Count;

            _indices[pos].AddRange(indicesCopy);
            _uvs[pos].AddRange(uvs);
            _normals[pos].AddRange(normals);
            chunk.MarkDirty();
        }
    }
}