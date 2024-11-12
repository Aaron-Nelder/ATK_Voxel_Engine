using ATKVoxelEngine.EditorManager;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(VoxelMeshData_SO))]
    public class VoxelMeshDataEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        VoxelMeshData_SO _target;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();

            _target = target as VoxelMeshData_SO;
            _inspectorXML.CloneTree(_inspector);

            Label titleLabel = _inspector.Q<Label>("Title");
            titleLabel.text = _target.name;

            Button saveMeshButton = _inspector.Q<Button>("SaveMeshButton");
            saveMeshButton.clicked += () => _target.SaveMesh();

            Button bakeDataButton = _inspector.Q<Button>("BakeDataButton");
            bakeDataButton.clicked += () => _target.BakeData();

            Button modifyMeshButton = _inspector.Q<Button>("ModifyMesh");
            modifyMeshButton.clicked += ModifyMesh;

            CreatePreviewImage(_inspector.Q<VisualElement>("Icon"), _target);

            // Return the finished Inspector UI.
            return _inspector;
        }

        void ModifyMesh()
        {
            foreach (GameObject customMesh in GameObject.FindGameObjectsWithTag(EngineConstants.CUSTOM_MESH_TAG))
                DestroyImmediate(customMesh);

            VoxelMeshData_SO.SpawnMeshForEdit(_target);
            EngineEditorManager.SetEditorMode(EditorMode.MESH_EDITOR);
        }

        public static void CreatePreviewImage(VisualElement preview, VoxelMeshData_SO meshData)
        {
            GameObject tempGO = new GameObject("TempMeshPreview");
            MeshFilter meshFilter = tempGO.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = tempGO.AddComponent<MeshRenderer>();
            tempGO.transform.position = new Vector3(-1000, -1000, -1000);
            tempGO.transform.localRotation = Quaternion.Euler(0, 45, 0);

            meshFilter.mesh = meshData.Mesh;
            meshRenderer.material = meshData.Material;

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
