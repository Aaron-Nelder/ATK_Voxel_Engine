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
                if(evt.newValue != null)
                {
                    meshDataPreview.style.display = DisplayStyle.Flex;
                    CreatePreviewImage(meshDataPreview);
                }
                else
                {
                    // remove mesh data editor
                    meshDataPreview.style.display = DisplayStyle.None;

                }

            });

            if(_target.MeshData != null)
            {

            }

            // Return the finished Inspector UI.
            return _inspector;
        }

        void CreatePreviewImage(VisualElement preview)
        {
            GameObject tempGO = new GameObject("TempMeshPreview");
            MeshFilter meshFilter = tempGO.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = tempGO.AddComponent<MeshRenderer>();
            tempGO.transform.position = new Vector3(-1000, -1000, -1000);
            tempGO.transform.localRotation = Quaternion.Euler(0, 45, 0);

            meshFilter.mesh = _target.MeshData.Mesh;
            meshRenderer.material = _target.MeshData.material;

            // Set up a camera to render the mesh
            Camera tempCamera = new GameObject("TempCamera").AddComponent<Camera>();
            tempCamera.transform.localRotation = Quaternion.Euler(25, 0, 0);
            tempCamera.clearFlags = CameraClearFlags.Color;
            tempCamera.backgroundColor = Color.clear;
            tempCamera.transform.position = new Vector3(-1000, -999f, -1002);
            tempCamera.targetTexture = new RenderTexture(256, 256, 16);

            // Render the mesh
            tempCamera.Render();

            // Capture the camera's output to a RenderTexture
            RenderTexture renderTexture = tempCamera.targetTexture;

            // Create a texture from the RenderTexture
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = null;

            // Create a VisualElement to display the texture
            preview.style.backgroundImage = new StyleBackground(texture);

            // Clean up temporary objects
            DestroyImmediate(tempGO);
            DestroyImmediate(tempCamera.gameObject);
        }
    }
}
