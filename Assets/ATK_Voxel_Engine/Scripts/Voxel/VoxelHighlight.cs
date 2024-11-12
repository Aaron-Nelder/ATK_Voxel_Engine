using Unity.Mathematics;
using UnityEngine;

namespace ATKVoxelEngine
{
    public class VoxelHighlight : ITickable
    {
        public TickType TickType => TickType.LATE_UPDATE;

        static RenderParams _renderParams;
        static Mesh _mesh;
        static Matrix4x4 _transformMatrix;
        static VoxelCastHit _selectedVoxel;
        static bool _drawWireframe;

        public VoxelHighlight()
        {
            _renderParams = new RenderParams(EngineSettings.DebugSettings.wireframeMaterial);
            _renderParams.receiveShadows = false;
            _renderParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _mesh = new Mesh();
            _transformMatrix = Matrix4x4.identity;
            _selectedVoxel = new VoxelCastHit();
            _drawWireframe = false;
            Register();
        }

        public void Register()
        {
            Selector.OnSelect += OnVoxelSelected;
            Selector.OnDeselect += HideWireFrame;
            TickRateManager.OnLateUpdate += Tick;
        }

        public void UnRegister()
        {
            Selector.OnSelect -= OnVoxelSelected;
            Selector.OnDeselect -= HideWireFrame;
            TickRateManager.OnLateUpdate -= Tick;
        }

        public void HideWireFrame(VoxelCastHit voxel) => _drawWireframe = false;

        public void Tick(float deltaTime)
        {
            if (!_drawWireframe) return;
            Graphics.RenderMesh(_renderParams, _mesh, 0, _transformMatrix);
        }

        static void OnVoxelSelected(VoxelCastHit newVoxel)
        {
            _drawWireframe = true;

            if (newVoxel.Id != _selectedVoxel.Id)
            {
                _mesh.Clear();
                _mesh = EngineSettings.GetVoxelData(newVoxel.Id).MeshData.MeshInstance;
            }

            _selectedVoxel = newVoxel;

            int3 pos = EngineUtilities.LocalPosToWorldPos(_selectedVoxel.Chunk.Position, _selectedVoxel.LocalPosition);
            _transformMatrix = Matrix4x4.identity;
            _transformMatrix.m03 = pos.x;
            _transformMatrix.m13 = pos.y;
            _transformMatrix.m23 = pos.z;
        }

        ~VoxelHighlight() => UnRegister();
    }
}
