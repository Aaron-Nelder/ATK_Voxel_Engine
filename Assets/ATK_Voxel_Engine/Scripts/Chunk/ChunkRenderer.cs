using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace ATKVoxelEngine
{
    public struct ChunkRenderer
    {
        // The dictionaries that holds the vertices, indices, uvs and normals for each voxel
        Dictionary<int3, List<Vector3>> _vertices;
        Dictionary<int3, List<int>> _indices;
        Dictionary<int3, List<Vector2>> _uvs;
        Dictionary<int3, List<int3>> _normals;
        int3 _chunkSize;

        // Initalizes the renderer with the given chunk
        public ChunkRenderer(Chunk chunk)
        {
            _vertices = new Dictionary<int3, List<Vector3>>();
            _indices = new Dictionary<int3, List<int>>();
            _uvs = new Dictionary<int3, List<Vector2>>();
            _normals = new Dictionary<int3, List<int3>>();
            _chunkSize = EngineSettings.WorldSettings.ChunkSize;
            InitalizeDictionaries();
            AddFacesFromInt(chunk);
            //RefreshVisibleVoxels(chunk);
        }

        // Fills the dictionarys with the voxels positions as keys
        void InitalizeDictionaries()
        {
            for (int x = 0; x < _chunkSize.x; x++)
            {
                for (int y = 0; y < _chunkSize.y; y++)
                {
                    for (int z = 0; z < _chunkSize.z; z++)
                    {
                        int3 pos = new(x, y, z);
                        _vertices.Add(pos, new());
                        _indices.Add(pos, new());
                        _uvs.Add(pos, new());
                        _normals.Add(pos, new());
                    }
                }
            }
        }

        // Re-Generates the mesh for the chunk with only the blocks that were visible before
        public void RefreshVisibleVoxels(Chunk chunk, bool applyData = false)
        {
            int3 chunkSize = EngineSettings.WorldSettings.ChunkSize;

            for (int x = 0; x < chunkSize.x; x++)
                for (int y = 0; y < chunkSize.y; y++)
                    for (int z = 0; z < chunkSize.z; z++)
                        AddVisibleFaces(chunk, new(x, y, z));

            if (applyData)
                ApplyData(chunk);
        }

        public void AddFacesFromInt(Chunk chunk)
        {
            NativeArray<int> visibleFaces = GetVisiblesList(chunk);

            for (int i = 0; i < visibleFaces.Length; i++)
            {
                if (visibleFaces[i] == 0) continue;

                int3 pos = new(i % EngineSettings.WorldSettings.ChunkSize.x, (i / EngineSettings.WorldSettings.ChunkSize.x) % EngineSettings.WorldSettings.ChunkSize.y, i / (EngineSettings.WorldSettings.ChunkSize.x * EngineSettings.WorldSettings.ChunkSize.y));
                VoxelData_SO data = EngineSettings.GetVoxelData(chunk.GetVoxel(pos));

                data.OldMeshData.GetPlanesFromInt(visibleFaces[i], pos, out List<Vector3> vertices, out List<int> indices, out List<Vector2> uvs, out List<int3> normals);

                SetData(chunk, pos, vertices, indices, data.ScaledUVs(uvs), normals);
            }

            visibleFaces.Dispose();
        }

        NativeArray<int> GetVisiblesList(Chunk chunk)
        {
            int3 chunkSize = EngineSettings.WorldSettings.ChunkSize;
            int length = chunkSize.x * chunkSize.y * chunkSize.z;
            NativeArray<uint> uints = new NativeArray<uint>(length, Allocator.TempJob);

            // flatten the 3D array into a 1D array
            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int y = 0; y < chunkSize.y; y++)
                {
                    for (int z = 0; z < chunkSize.z; z++)
                    {
                        int index = x + y * chunkSize.x + z * chunkSize.x * chunkSize.y;
                        uints[index] = chunk.GetVoxel(x, y, z);
                    }
                }
            }

            NativeArray<int> result = new NativeArray<int>(length, Allocator.Persistent);
            VisibleVoxelsJob job = new VisibleVoxelsJob
            {
                result = result,
                chunkSize = EngineSettings.WorldSettings.ChunkSize,
                VoxelIds = uints
            };

            JobHandle handle = job.ScheduleByRef();
            handle.Complete();

            uints.Dispose();
            return result;
        }

        //Write me a function that only refreshes the border voxels
        public void RefreshBorderVoxels(Chunk chunk)
        {
            int3 chunkSize = EngineSettings.WorldSettings.ChunkSize;

            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int y = 0; y < chunkSize.y; y++)
                {
                    for (int z = 0; z < chunkSize.z; z++)
                    {
                        if (x == 0 || x == chunkSize.x - 1 || y == 0 || y == chunkSize.y - 1 || z == 0 || z == chunkSize.z - 1)
                            AddVisibleFaces(chunk, new(x, y, z));
                    }
                }
            }
        }

        // Applies the combined vertices, indices, uvs and normals to one signal mesh and applies it to the mesh filter and collider
        public void ApplyData(Chunk chunk, Action callback = null)
        {
            List<Vector3> vertValues = new List<Vector3>();
            foreach (var v in _vertices)
                vertValues.AddRange(v.Value);

            int vertOffset = 0;
            List<int> indicesValues = new List<int>();
            foreach (var b in _indices)
            {
                foreach (var i in b.Value)
                    indicesValues.Add(i + vertOffset);
                vertOffset += b.Value.Count;
            }

            List<Vector2> uvValues = new List<Vector2>();
            foreach (var v in _uvs)
                uvValues.AddRange(v.Value);

            List<Vector3> normValues = new List<Vector3>();
            foreach (var b in _normals)
                foreach (var n in b.Value)
                    normValues.Add(new(n.x, n.y, n.z));

            /*
            chunk.Filter.sharedMesh.Clear();
            chunk.Filter.sharedMesh.SetVertices(vertValues);
            chunk.Filter.sharedMesh.SetIndices(indicesValues, MeshTopology.Quads, 0);
            chunk.Filter.sharedMesh.SetUVs(0, uvValues);
            chunk.Filter.sharedMesh.SetNormals(normValues);
            chunk.Filter.sharedMesh.RecalculateBounds();

            chunk.Collider.sharedMesh = chunk.Filter.sharedMesh;
            chunk.IsDirty = false;
            callback?.Invoke();
            */
        }

        public void AddVisibleFaces(Chunk chunk, int3 pos)
        {
            if (chunk.GetVoxel(pos) == 0) return;
            VoxelData_SO data = EngineSettings.GetVoxelData(chunk.GetVoxel(pos));

            if (!data.OldMeshData.GetVisiblesPlanes(chunk.Position, pos, out List<Vector3> vertices, out List<int> indices, out List<Vector2> uvs, out List<int3> normals))
                return;

            SetData(chunk, pos, vertices, indices, data.ScaledUVs(uvs), normals);
        }

        public void AddVoxelFace(Chunk chunk, int3 pos, int3 faceNormal)
        {
            uint id = chunk.GetVoxel(pos);
            if (id == 0) return;
            VoxelData_SO data = EngineSettings.GetVoxelData(id);
            data.OldMeshData.GetPlane(pos, faceNormal, out List<Vector3> vertices, out List<int> indices, out List<Vector2> uvs, out List<int3> normals);

            AddData(chunk, pos, vertices, indices, data.ScaledUVs(uvs), normals);
        }

        // Removes the given block from the mesh data
        public void ClearData(Chunk chunk, int3 pos)
        {
            _vertices[pos].Clear();
            _indices[pos].Clear();
            _uvs[pos].Clear();
            _normals[pos].Clear();
        }

        void SetData(Chunk chunk, int3 pos, List<Vector3> vertices, List<int> indices, List<Vector2> uvs, List<int3> normals)
        {
            _vertices[pos] = vertices;

            List<int> indicesCopy = new List<int>(indices);
            for (int i = 0; i < indices.Count; i++)
                indicesCopy[i] = indicesCopy[i] + _vertices[pos].Count - vertices.Count;

            _indices[pos] = indicesCopy;
            _uvs[pos] = uvs;
            _normals[pos] = normals;
        }

        void AddData(Chunk chunk, int3 pos, List<Vector3> vertices, List<int> indices, List<Vector2> uvs, List<int3> normals)
        {
            _vertices[pos].AddRange(vertices);

            List<int> indicesCopy = new List<int>(indices);
            for (int i = 0; i < indices.Count; i++)
                indicesCopy[i] = indicesCopy[i] + _vertices[pos].Count - vertices.Count;

            _indices[pos].AddRange(indicesCopy);
            _uvs[pos].AddRange(uvs);
            _normals[pos].AddRange(normals);
        }
    }
}