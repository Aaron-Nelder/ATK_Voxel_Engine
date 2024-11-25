using UnityEngine;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Voxel Data", menuName = EngineConstants.ENGINE_NAME + "/Voxel Data")]
    public class VoxelData_SO : ScriptableObject
    {
        [SerializeField] VoxelType _type;
        public VoxelType Id => _type;

        public string Name => _type.ToString();

        [SerializeField] VoxelMeshData_SO _meshData;
        public VoxelMeshData_SO MeshData => _meshData;

        [SerializeField] bool _hasCollision = true;
        public bool HasCollision => _hasCollision;

        [SerializeField] bool _isTransparent = false;
        public bool IsTransparent => _isTransparent;

        [SerializeField] bool _isDirectionBased = false;
        public bool UsesDirection => _isDirectionBased;
    }
}