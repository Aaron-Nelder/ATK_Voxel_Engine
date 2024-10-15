using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VoxelHighlight : MonoBehaviour
{
    [SerializeField] Material _highlightMaterial;
    MeshFilter _filter;
    MeshRenderer _renderer;
    SelectedVoxel _selectedVoxel;

    Mesh _mesh;

    public VoxelHighlight Init()
    {
        _mesh = new Mesh();
        _filter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        _renderer.sharedMaterial = _highlightMaterial;
        _filter.sharedMesh = _mesh;
        _renderer.enabled = false;
        return this;
    }

    void OnEnable()
    {
        Selector.OnSelect += OnVoxelSelected;
        Selector.OnDeselect += DisableRenderer;
    }

    void OnDisable()
    {
        Selector.OnSelect -= OnVoxelSelected;
        Selector.OnDeselect -= DisableRenderer;
    }

    void OnVoxelSelected(SelectedVoxel newVoxel)
    {
        _renderer.enabled = true;
        if (newVoxel.Id != _selectedVoxel.Id)
        {
            _mesh.Clear();
            _mesh = VoxelManager.GetVoxelData(newVoxel.Id).MeshData.Mesh;
            _filter.sharedMesh = _mesh;
        }

        _selectedVoxel = newVoxel;
        transform.position = WorldHelper.LocalPosToWorldPos(_selectedVoxel.Chunk.Position, _selectedVoxel.LocalPosition);
    }

    void DisableRenderer(SelectedVoxel newVoxel)
    {
        _renderer.enabled = false;
    }
}
