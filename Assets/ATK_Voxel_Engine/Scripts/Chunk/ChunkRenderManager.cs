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
        VertexAttributeDescriptor[] _descriptors = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, dimension: 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2)
        };

        Mesh _mesh;
        Bounds _bounds;
        bool _isSetup = false;
        JobHandle _collisionBakeHandle;

        // Tasks
        Task _collisionTask;
        Task _meshTask;

        public void Initialize(Chunk chunk)
        {
#if UNITY_EDITOR
            runInEditMode = true;
#endif
            _initialized = false;

            if (!_isSetup)
                SetUp(chunk);

            AddVisibleFaces(_chunk);

            SetMeshBuffer();
        }

        void SetUp(Chunk chunk)
        {
            _chunkSize = EngineSettings.WorldSettings.ChunkSize;
            _renderParams = new RenderParams(_material);
            _renderParams.receiveShadows = true;
            _renderParams.shadowCastingMode = ShadowCastingMode.On;
            _chunk = chunk;
            _bounds = EngineSettings.WorldSettings.ChunkBounds;
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

        // Adds the visible faces to the dictionaries
        void AddVisibleFaces(Chunk chunk)
        {
            // Gets the list of visible faces and storing those values in an int array
            int length = _chunkSize.x * _chunkSize.y * _chunkSize.z;
            NativeArray<int> visibleFaces = new NativeArray<int>(length, Allocator.Persistent);
            new VisibleVoxelsJob(chunk.GetVoxels(), visibleFaces, _chunkSize).Schedule().Complete();

            for (int i = 0; i < visibleFaces.Length; i++)
            {
                int3 pos = EngineUtilities.GetVoxelPos(i, _chunkSize);
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

        // Sets the mesh buffer
        public async void SetMeshBuffer()
        {
            await Awaitable.MainThreadAsync();

            if (!_isSetup)
            {
                _mesh = new Mesh();
                _mesh.MarkDynamic();
                _mesh.bounds = _bounds;
                _isSetup = true;
            }

            MeshDataArray meshDataArray = AllocateWritableMeshData(1);

            int totalVertices = _vertices.Sum(v => v.Value.Length);

            _meshTask = Task.Factory.StartNew(() => FillMeshBuffer(meshDataArray[0], totalVertices));
            await _meshTask;

            await Awaitable.MainThreadAsync();

            ApplyAndDisposeWritableMeshData(meshDataArray, _mesh, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            int instanceID = _mesh.GetInstanceID();
            _collisionTask = Task.Factory.StartNew(() => Physics.BakeMesh(instanceID, false));
            await _collisionTask;

            _collider.sharedMesh = _mesh;
            _initialized = true;
        }

        // Fills the mesh buffers with the vertex data stored in the dictionary
        void FillMeshBuffer(MeshData meshData,int totalVertices)
        {
            int totalIndices = totalVertices / MeshPlane.VERTEX_COUNT * MeshPlane.INDEX_COUNT;
            _transformMatrix = GetTransformationMatrix(_chunk.WorldPosition);

            meshData.SetVertexBufferParams(totalVertices, _descriptors);
            meshData.SetIndexBufferParams(totalIndices, IndexFormat.UInt32);

            _vertexBufferData = meshData.GetVertexData<Vertex>();
            _indicesBufferData = meshData.GetIndexData<uint>();

            DictToArrays(_vertices, _vertexBufferData, _indicesBufferData);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, totalIndices));
        }

        public void RemoveVoxelVertices(int3 vPos) => _vertices[vPos] = new Vertex[0];

        public void AddVoxelFace(int3 pos, int3 faceNormal)
        {
            uint id = _chunk.GetVoxel(pos);
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

        public void OnRemoveVoxelFace(int3 vPos, int3 normal)
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
            _meshTask?.Dispose();
            _collisionTask?.Dispose();
            _initialized = false;
        }
    }
}