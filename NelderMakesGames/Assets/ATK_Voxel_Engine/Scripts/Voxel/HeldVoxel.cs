using System.Linq;
using UnityEngine;

public class HeldVoxel: MonoBehaviour 
{
    Transform _handTransform;
    VoxelData_SO _data;
    public uint Id => _data.Id;

    [SerializeField] MeshFilter _filter;
    [SerializeField] MeshRenderer _renderer;

    public HeldVoxel Init(Transform handTransform, VoxelData_SO data)
    {
        _handTransform = handTransform;
        _data = data;
        AssignMesh(data);
        return this;
    }

    void AssignMesh(VoxelData_SO data)
    {
        _filter.mesh = data.MeshData.Mesh;
        _filter.mesh.SetUVs(0, data.ScaledUVs(_filter.mesh.uv.ToList()));
        _renderer.material = VoxelManager.MaterialAtlas[0].Material;
    }

}
