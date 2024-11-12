using UnityEngine;

namespace ATKVoxelEngine
{
    [CreateAssetMenu(fileName = "Voxel Data", menuName = EngineConstants.ENGINE_NAME + "/Voxel Data")]
    public class VoxelData_SO : ScriptableObject
    {
        [SerializeField] uint _id;
        public uint Id => _id;

        [SerializeField] string _name;
        public string Name => _name;

        [SerializeField] VoxelMeshData_SO _meshData;
        public VoxelMeshData_SO MeshData => _meshData;
    }
}