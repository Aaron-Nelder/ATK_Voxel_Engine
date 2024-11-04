using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

namespace ATKVoxelEngine
{
    [RequireComponent(typeof(MeshCollider))]
    public class ChunkRenderManager : MonoBehaviour
    {
        [SerializeField] Material _material;
        [SerializeField] MeshCollider _collider;
        Chunk _chunk;

        bool _initialized = false;
        int3 _chunkSize;

        RenderParams _renderParams;
        Matrix4x4 _transformMatrix;

        // MeshData Dictionaries
        Dictionary<int3, Vertex[]> _vertices = new Dictionary<int3, Vertex[]>();

        // MeshData Buffers
        NativeArray<Vertex> _vertexBufferData;
        NativeArray<uint> _indicesBufferData;

        Mesh _mesh;
        bool _isSetup = false;

        public void Initialize(Chunk chunk)
        {
            _initialized = false;

            if (!_isSetup)
                SetUp(chunk);

            AddVisibleFaces(_chunk);

            int3 chunkWorldPos = new int3(_chunk.WorldPosition.x, _chunk.WorldPosition.y, _chunk.WorldPosition.z);
            _transformMatrix = GetTransformationMatrix(chunkWorldPos);

            SetMeshBuffer();

            _initialized = true;
        }

        void SetUp(Chunk chunk)
        {
            _chunkSize = EngineSettings.WorldSettings.ChunkSize;
            _renderParams = new RenderParams(_material);
            _chunk = chunk;
            InitializeDicts(_chunkSize);
        }

        Matrix4x4 GetTransformationMatrix(int3 chunkPos)
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.m03 = chunkPos.x;
            m.m13 = chunkPos.y;
            m.m23 = chunkPos.z;
            return m;
        }

        // Initializes the dictionaries sizes and lists
        void InitializeDicts(int3 chunkSize)
        {
            for (int x = 0; x < chunkSize.x; x++)
                for (int y = 0; y < chunkSize.y; y++)
                    for (int z = 0; z < chunkSize.z; z++)
                        _vertices.Add(new int3(x, y, z), new Vertex[0]);
        }

        // returns a NativeArray with the visible faces voxels within the chunk
        NativeArray<int> GetVisiblesList(Chunk chunk)
        {
            // flatten the 3D array into a 1D array
            int length = _chunkSize.x * _chunkSize.y * _chunkSize.z;
            NativeArray<int> result = new NativeArray<int>(length, Allocator.Persistent);

            JobHandle handle = new VisibleVoxelsJob(chunk.GetVoxels(), result, _chunkSize).Schedule();
            handle.Complete();

            return result;
        }

        // Adds the visible faces to the dictionaries
        void AddVisibleFaces(Chunk chunk)
        {
            NativeArray<int> visibleFaces = GetVisiblesList(chunk);

            for (int i = 0; i < visibleFaces.Length; i++)
            {
                int3 pos = WorldHelper.GetVoxelPos(i, _chunkSize);
                if (visibleFaces[i] == 0)
                {
                    _vertices[pos] = new Vertex[0];
                    continue; // if there are no visible faces, skip
                }

                VoxelData_SO data = EngineSettings.GetVoxelData(chunk.GetVoxel(pos));

                data.MeshData.GetVisiblePlanes(visibleFaces[i], pos, out Vertex[] vertices);
                _vertices[pos] = vertices;
            }

            visibleFaces.Dispose();
        }

        // Converts the dictionary to NativeArrays
        void DictToArrays(Dictionary<int3, Vertex[]> verticesDict, ref NativeArray<Vertex> outVertices, ref NativeArray<uint> outIndices)
        {
            int vertexIndex = 0;
            foreach (var kvp in verticesDict)
                foreach (var vertex in kvp.Value)
                    outVertices[vertexIndex++] = vertex;

            int indexIndex = 0;
            uint vertOffset = 0;
            for (int i = 0; i < outIndices.Length; i++)
            {
                outIndices[i] = MeshPlane.Indices[indexIndex++] + vertOffset;
                if (indexIndex == MeshPlane.INDEX_COUNT)
                {
                    indexIndex = 0;
                    vertOffset += MeshPlane.VERTEX_COUNT;
                }
            }
        }

        // Sets the mesh buffer
        public async void SetMeshBuffer()
        {
            var descriptors = new VertexAttributeDescriptor[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2)
            };

            int totalVertices = _vertices.Values.Sum(list => list.Length);
            int totalIndices = totalVertices / MeshPlane.VERTEX_COUNT * MeshPlane.INDEX_COUNT;

            await Awaitable.MainThreadAsync();

            MeshDataArray meshDataArray = AllocateWritableMeshData(1);
            MeshData meshData = meshDataArray[0];

            meshData.SetVertexBufferParams(totalVertices, descriptors);
            meshData.SetIndexBufferParams(totalIndices, IndexFormat.UInt32);

            _vertexBufferData = meshData.GetVertexData<Vertex>();
            _indicesBufferData = meshData.GetIndexData<uint>();

            DictToArrays(_vertices, ref _vertexBufferData, ref _indicesBufferData);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, totalIndices));

            if (!_isSetup)
            {
                _mesh = new Mesh();
                _mesh.bounds = EngineSettings.WorldSettings.ChunkBounds;
                _isSetup = true;
            }

            ApplyAndDisposeWritableMeshData(meshDataArray, _mesh, MeshUpdateFlags.DontRecalculateBounds);

            _collider.sharedMesh = _mesh;
            _vertexBufferData.Dispose();
            _indicesBufferData.Dispose();
            _initialized = true;
        }

        public void RemoveVoxel(int3 vPos)
        {
            _vertices[vPos] = new Vertex[0];
            SetMeshBuffer();
        }

        public void AddVoxelFace(int3 pos, int3 faceNormal)
        {
            uint id = _chunk.GetVoxel(pos);
            if (id == 0) return;

            VoxelData_SO data = EngineSettings.GetVoxelData(id);
            Vertex[] planes = data.MeshData.GetPlanes(faceNormal);

            for (int i = 0; i < planes.Length; i++)
                planes[i].position += pos;

            Vertex[] newVertices = new Vertex[_vertices[pos].Length + planes.Length];
            _vertices[pos].CopyTo(newVertices, 0);
            planes.CopyTo(newVertices, _vertices[pos].Length);

            _vertices[pos] = newVertices;
        }

        public void OnRemoveVoxelFace(int3 vPos,int3 normal)
        {
            _vertices[vPos] = _vertices[vPos].Where(v => !v.normal.Equals(normal)).ToArray();
        }

        void Update()
        {
            if (!_initialized || !_isSetup) return;

            Graphics.RenderMesh(_renderParams, _mesh, 0, _transformMatrix);
        }

        void OnDisable()
        {
            _initialized = false;
        }
    }
}