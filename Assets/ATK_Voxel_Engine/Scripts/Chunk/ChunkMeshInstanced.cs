using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

namespace ATKVoxelEngine
{
    public class ChunkMeshInstanced : MonoBehaviour
    {
        [SerializeField] ComputeShader _matrixComputeShader;

        // buffers
        ComputeBuffer _voxelsBuffer, _visibleVoxelMatrixBuffer, _visibilityResultsBuffer;

        Dictionary<uint, List<Matrix4x4>> _matricesDic = new Dictionary<uint, List<Matrix4x4>>();

        bool _initialized = false;
        int3 _chunkSize;
        ChunkPosition _chunkPos;

        int _numVoxels;
        uint[] _voxelsFlat, _visResults;
        Matrix4x4[] _matrices;

        public async void Initialize(Chunk chunk)
        {
            SetUpArrays(chunk);

            await Awaitable.MainThreadAsync();

            SetConstants();
            CreateBuffers();
            SetBuffers(ref _voxelsFlat, ref _matrices, ref _visResults);
            DispatchShader();

            _initialized = true;
        }

        // Initalizes the size of the arrays
        void SetUpArrays(Chunk chunk)
        {
            _numVoxels = _chunkSize.x * _chunkSize.y * _chunkSize.z;
            _voxelsFlat = new uint[_numVoxels];
            _matrices = new Matrix4x4[_numVoxels];
            _visResults = new uint[_numVoxels];

            // fills the voxelsFlat array
            for (int x = 0; x < _chunkSize.x; x++)
                for (int y = 0; y < _chunkSize.y; y++)
                    for (int z = 0; z < _chunkSize.z; z++)
                        _voxelsFlat[x + y * _chunkSize.x + z * _chunkSize.x * _chunkSize.y] = chunk.GetVoxel(x, y, z);
        }

        // Sets the constants for the compute shader
        void SetConstants()
        {
            _matrixComputeShader.SetInts("ChunkSize", _chunkSize.x, _chunkSize.y, _chunkSize.z);
            _matrixComputeShader.SetInts("ChunkPosition", _chunkPos.x, _chunkPos.z);
            _matrixComputeShader.SetInts("AllVoxelCount", _numVoxels);
        }

        // Creates the compute buffers
        void CreateBuffers()
        {
            _voxelsBuffer = new ComputeBuffer(_numVoxels, sizeof(uint));
            _visibleVoxelMatrixBuffer = new ComputeBuffer(_numVoxels, sizeof(float) * 16);
            _visibilityResultsBuffer = new ComputeBuffer(_numVoxels, sizeof(uint));
        }

        // sets the buffers for the compute shader
        void SetBuffers(ref uint[] voxelsFlat, ref Matrix4x4[] matrices, ref uint[] visResults)
        {
            int kernel = _matrixComputeShader.FindKernel("CSMain");

            _voxelsBuffer.SetData(voxelsFlat);
            _visibleVoxelMatrixBuffer.SetData(matrices);
            _visibilityResultsBuffer.SetData(visResults);

            // Sets the buffers and parameters to the compute shader
            _matrixComputeShader.SetBuffer(kernel, "Voxels", _voxelsBuffer);
            _matrixComputeShader.SetBuffer(kernel, "VisibleVoxelMatrix", _visibleVoxelMatrixBuffer);
            _matrixComputeShader.SetBuffer(kernel, "VoxelVisibility", _visibilityResultsBuffer);
        }

        void DispatchShader()
        {
            int kernel = _matrixComputeShader.FindKernel("CSMain");

            // Dispatches the compute shader
            int threadGroupSizeX = Mathf.CeilToInt(_chunkSize.x / 8f);
            int threadGroupSizeY = Mathf.CeilToInt(_chunkSize.y / 8f);
            int threadGroupSizeZ = Mathf.CeilToInt(_chunkSize.z / 8f);
            _matrixComputeShader.Dispatch(kernel, threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ);

            // Gets the results from the buffers
            _visibleVoxelMatrixBuffer.GetData(_matrices);
            _visibilityResultsBuffer.GetData(_visResults);

            // Sets the matrices
            for (int i = 0; i < _voxelsFlat.Length; i++)
            {
                if (_visResults[i] != 0)
                {
                    if (_matricesDic.ContainsKey(_voxelsFlat[i]))
                        _matricesDic[_voxelsFlat[i]].Add(_matrices[i]);
                    else
                        _matricesDic.TryAdd(_voxelsFlat[i], new List<Matrix4x4>() { _matrices[i] });
                }
            }
        }

        void Update()
        {
            if (!_initialized) return;

            foreach (var key in _matricesDic.Keys)
            {
                Mesh mesh = EngineSettings.VoxelAtlas[key].MeshData.Mesh;
                Material material = EngineSettings.VoxelAtlas[key].MeshData.Material;
                Graphics.DrawMeshInstanced(mesh, 0, material, _matricesDic[key]);
            }
        }

        void OnDisable()
        {
            // Disposes the buffers
            _voxelsBuffer.Dispose();
            _visibleVoxelMatrixBuffer.Dispose();
            _visibilityResultsBuffer.Dispose();
            _initialized = false;
        }
    }
}
