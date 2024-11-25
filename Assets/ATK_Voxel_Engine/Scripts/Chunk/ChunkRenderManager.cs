using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

namespace ATKVoxelEngine
{
    [RequireComponent(typeof(MeshCollider))]
    public class ChunkRenderManager : MonoBehaviour, IDisposable
    {
        [SerializeField] Material _material;
        [SerializeField] MeshCollider _collider;
        Chunk _chunk;

        bool _isSetup = false, _initialized = false;
        int3 _chunkSize;

        RenderParams _renderParams;
        Matrix4x4 _transformMatrix;

        // MeshData Dictionaries
        Dictionary<int3, Vertex[]> _vertices = new Dictionary<int3, Vertex[]>();

        // MeshData Buffers
        NativeArray<Vertex> _vertexBufferData;
        NativeArray<uint> _indicesBufferData;
        VertexAttributeDescriptor[] _descriptors = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2)
        };

        Mesh _mesh;

        public async Task Initialize(Chunk chunk)
        {
            _initialized = false;
            _chunk = chunk;

            if (!_isSetup)
                await FirstTimeSetup(chunk);

            await AddVisibleFaces(_chunk);

            await SetMeshBuffer();
        }

        async Task FirstTimeSetup(Chunk chunk)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                _chunkSize = EngineSettings.WorldSettings.ChunkSize;
                _renderParams = new RenderParams(_material);
                _renderParams.receiveShadows = true;
                _renderParams.shadowCastingMode = ShadowCastingMode.On;
                _chunk = chunk;

                // fill the dictionary with the chunk size
                for (int x = 0; x < _chunkSize.x; x++)
                    for (int y = 0; y < _chunkSize.y; y++)
                        for (int z = 0; z < _chunkSize.z; z++)
                            _vertices.Add(new int3(x, y, z), new Vertex[0]);

            }, TaskCreationOptions.None);

            await TaskUtility.AwaitTask(task, destroyCancellationToken);

            _mesh = new Mesh();
            _mesh.MarkDynamic();
            _isSetup = true;
        }

        Matrix4x4 GetTransformationMatrix(int3 chunkPos)
        {
            Matrix4x4 m = Matrix4x4.identity;
            m.m03 = chunkPos.x;
            m.m13 = chunkPos.y;
            m.m23 = chunkPos.z;
            return m;
        }

        // Adds the visible faces of each voxel to the dictionary
        async Task AddVisibleFaces(Chunk chunk)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                // Gets the list of visible faces and storing those values in an int array
                int length = _chunkSize.x * _chunkSize.y * _chunkSize.z;
                NativeArray<int> visibleFaces = new NativeArray<int>(length, Allocator.Persistent);
                new VisibleVoxelsJob(chunk.Data.Voxels, visibleFaces, _chunkSize).Schedule().Complete();

                for (int i = 0; i < visibleFaces.Length; i++)
                {
                    int3 pos = EngineUtilities.GetVoxelPos(i, _chunkSize);
                    if (visibleFaces[i] == 0)
                    {
                        _vertices[pos] = new Vertex[0];
                        continue; // if there are no visible faces, skip
                    }

                    VoxelData_SO data = EngineSettings.GetVoxelData(chunk.Data.GetVoxel(pos));
                    data.MeshData.GetVisiblePlanes(visibleFaces[i], pos, out Vertex[] vertices);
                    _vertices[pos] = vertices;
                }

                visibleFaces.Dispose();
            }, TaskCreationOptions.LongRunning);

            await TaskUtility.AwaitTask(task, destroyCancellationToken);
        }

        // Converts the dictionary to NativeArrays
        void DictToArrays(Dictionary<int3, Vertex[]> verticesDict, NativeArray<Vertex> outVertices, NativeArray<uint> outIndices)
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

        // Applies the vertex data from the dictionary to the mesh and bakes the collision mesh
        public async Task SetMeshBuffer()
        {
            MeshDataArray meshDataArray = AllocateWritableMeshData(1);

            await FillMeshBuffer(meshDataArray[0]);

            ApplyAndDisposeWritableMeshData(meshDataArray, _mesh, MeshUpdateFlags.DontValidateIndices);

            await BakeCollision(_mesh);

            _mesh.RecalculateBounds();
            _collider.sharedMesh = _mesh;
            _initialized = true;

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                runInEditMode = true;
#endif
        }

        async Task BakeCollision(Mesh mesh)
        {
            int id = mesh.GetInstanceID();
            Task task = Task.Factory.StartNew(() =>
            {
                Physics.BakeMesh(id, false);
            }, TaskCreationOptions.None);

            await TaskUtility.AwaitTask(task, destroyCancellationToken);
        }

        // Fills the mesh buffers with the vertex data stored in the dictionary
        async Task FillMeshBuffer(MeshData meshData)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                int totalVertices = _vertices.Sum(v => v.Value.Length);
                int totalIndices = totalVertices / MeshPlane.VERTEX_COUNT * MeshPlane.INDEX_COUNT;
                _transformMatrix = GetTransformationMatrix(_chunk.Data.WorldPosition);

                meshData.SetVertexBufferParams(totalVertices, _descriptors);
                meshData.SetIndexBufferParams(totalIndices, IndexFormat.UInt32);

                _vertexBufferData = meshData.GetVertexData<Vertex>();
                _indicesBufferData = meshData.GetIndexData<uint>();

                DictToArrays(_vertices, _vertexBufferData, _indicesBufferData);

                meshData.subMeshCount = 1;
                meshData.SetSubMesh(0, new SubMeshDescriptor(0, totalIndices));

            }, TaskCreationOptions.None);

            await TaskUtility.AwaitTask(task, destroyCancellationToken);
        }

        public void RemoveVoxel(int3 vPos) => _vertices[vPos] = new Vertex[0];

        public void AddVoxelFace(int3 pos, int3 faceNormal)
        {
            VoxelType id = _chunk.Data.GetVoxel(pos);
            if (id == 0) return;

            VoxelData_SO data = EngineSettings.GetVoxelData(id);
            Vertex[] planes = data.MeshData.GetPlanesFromNormal(faceNormal);

            for (int i = 0; i < planes.Length; i++)
                planes[i].position += pos;

            Vertex[] newVertices = new Vertex[_vertices[pos].Length + planes.Length];
            _vertices[pos].CopyTo(newVertices, 0);
            planes.CopyTo(newVertices, _vertices[pos].Length);

            _vertices[pos] = newVertices;
        }

        public void RemoveVoxelFace(int3 vPos, int3 normal)
        {
            _vertices[vPos] = _vertices[vPos].Where(v => !v.normal.Equals(normal)).ToArray();
        }

        void Update()
        {
            if (!_initialized || !_isSetup) return;

            // check to se if the chunk is in the camera view
            //if (EngineUtilities.IsChunkVisible(_collider.bounds))
            Graphics.RenderMesh(_renderParams, _mesh, 0, _transformMatrix);
        }

        void OnDisable() => Dispose();

        public void Dispose()
        {
            if (_mesh != null)
                _mesh.Clear();
            _collider.sharedMesh = null;
            _initialized = false;
        }
    }
}