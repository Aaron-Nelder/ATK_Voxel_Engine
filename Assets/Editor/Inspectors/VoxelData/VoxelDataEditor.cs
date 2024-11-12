using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(VoxelData_SO))]
    public class VoxelDataEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        VoxelData_SO _target;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();

            _target = target as VoxelData_SO;
            _inspectorXML.CloneTree(_inspector);

            ObjectField meshDataField = _inspector.Q<ObjectField>("MeshDataField");
            VisualElement meshDataPreview = _inspector.Q<VisualElement>("MeshDataPreview");
            meshDataField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null)
                {
                    meshDataPreview.style.display = DisplayStyle.Flex;
                    VoxelMeshDataEditor.CreatePreviewImage(meshDataPreview, _target.MeshData);
                }
                else
                {
                    // remove mesh data editor
                    meshDataPreview.style.display = DisplayStyle.None;
                }

            });

            if (_target.MeshData != null)
            {

            }

            // Return the finished Inspector UI.
            return _inspector;
        }
    }
}
