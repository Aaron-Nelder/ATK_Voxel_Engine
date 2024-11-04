using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    public class WorldEditor : EditorWindow
    {
        WorldSettings_SO WorldSettings => EngineSettings.WorldSettings;
        DebugSettings_SO DebugSettings => EngineSettings.DebugSettings;
        [SerializeField] VisualTreeAsset visualTree = default;

        enum Menus { WORLD_SETTINGS = 0, DEBUG_UI = 1, PREVIEW_CHUNK = 2, MESH_CREATOR = 3, VOXEL_DATA = 4 }
        ToolbarButton[] toolbarButtons = new ToolbarButton[5];
        VisualElement[] menus = new VisualElement[5];

        CustomMesh_SO previewMesh;
        GameObject spawnedMesh;

        [MenuItem(EngineConstants.ENGINE_EDITOR_WINDOW_PATH)]
        public static void ShowExample()
        {
            WorldEditor wnd = GetWindow<WorldEditor>();
            wnd.titleContent = new GUIContent("WorldEditor");
        }

        public void CreateGUI()
        {
            // Instantiate UXML
            VisualElement uxml = visualTree.Instantiate();
            rootVisualElement.Add(uxml);

            SceneView.duringSceneGui += OnSceneGUI;
            GetMenuButtons();
            GetMenus();
            SetMenu(0);
        }

        void GetMenuButtons()
        {
            toolbarButtons[(int)Menus.WORLD_SETTINGS] = rootVisualElement.Q<ToolbarButton>("WorldSettingsButton");
            toolbarButtons[(int)Menus.WORLD_SETTINGS].clicked += () => { SetMenu((int)Menus.WORLD_SETTINGS); };
            toolbarButtons[(int)Menus.DEBUG_UI] = rootVisualElement.Q<ToolbarButton>("DebugUIButton");
            toolbarButtons[(int)Menus.DEBUG_UI].clicked += () => { SetMenu((int)Menus.DEBUG_UI); };
            toolbarButtons[(int)Menus.PREVIEW_CHUNK] = rootVisualElement.Q<ToolbarButton>("PreviewButton");
            toolbarButtons[(int)Menus.PREVIEW_CHUNK].clicked += () => { SetMenu((int)Menus.PREVIEW_CHUNK); };
            toolbarButtons[(int)Menus.MESH_CREATOR] = rootVisualElement.Q<ToolbarButton>("MeshCreatorButton");
            toolbarButtons[(int)Menus.MESH_CREATOR].clicked += () => { SetMenu((int)Menus.MESH_CREATOR); };
            toolbarButtons[(int)Menus.VOXEL_DATA] = rootVisualElement.Q<ToolbarButton>("VoxelDataButton");
            toolbarButtons[(int)Menus.VOXEL_DATA].clicked += () => { SetMenu((int)Menus.VOXEL_DATA); };
        }

        void GetMenus()
        {
            ScrollView scrollView = rootVisualElement.Q<ScrollView>("PageView");

            if (WorldSettings != null)
            {
                // Spawn the world settings menu
                WorldSettingsEditor worldSettingsEditor = ScriptableObject.CreateInstance<WorldSettingsEditor>();
                menus[(int)Menus.WORLD_SETTINGS] = worldSettingsEditor.CreateInspectorGUI();
                worldSettingsEditor.InitElements(WorldSettings);
                menus[(int)Menus.WORLD_SETTINGS].Bind(new SerializedObject(WorldSettings));
                scrollView.Add(menus[(int)Menus.WORLD_SETTINGS]);
            }

            if (DebugSettings != null)
            {
                DebugSettingsEditor debugSettingsEditor = ScriptableObject.CreateInstance<DebugSettingsEditor>();
                menus[(int)Menus.DEBUG_UI] = debugSettingsEditor.CreateInspectorGUI();
                debugSettingsEditor.InitElements(DebugSettings);
                menus[(int)Menus.DEBUG_UI].Bind(new SerializedObject(DebugSettings));
                scrollView.Add(menus[(int)Menus.DEBUG_UI]);
            }

            menus[(int)Menus.PREVIEW_CHUNK] = rootVisualElement.Q<VisualElement>("Preview");
            rootVisualElement.Q<Button>("PreviewChunk").clicked += () => { ChunkManager.PreviewChunkEditor(DebugHelper.CameraChunk); };
            rootVisualElement.Q<Button>("UnloadPreviewChunk").clicked += () =>
            {
                ChunkManager.Dispose(true);

                GameObject[] loadedChunks = GameObject.FindGameObjectsWithTag("Chunk");
                foreach (var chunk in loadedChunks)
                    DestroyImmediate(chunk);
            };

            rootVisualElement.Q<Button>("SaveMesh").clicked += () =>
            {
                previewMesh = rootVisualElement.Q<ObjectField>("CustomMesh").value as CustomMesh_SO;
                Mesh mesh = previewMesh.SaveMesh();
            };

            rootVisualElement.Q<Button>("PreviewMesh").clicked += () =>
            {
                if (spawnedMesh is null)
                {
                    Selection.activeGameObject = MeshCreator.SpawnMesh(rootVisualElement.Q<ObjectField>("CustomMesh").value as CustomMesh_SO, rootVisualElement.Q<ObjectField>("MeshMaterial").value as Material);
                }
                else if (!MeshCreator.DestroyMesh())
                {
                    DestroyImmediate(spawnedMesh);
                }
            };

            menus[(int)Menus.MESH_CREATOR] = rootVisualElement.Q<VisualElement>("MeshCreator");
            menus[(int)Menus.VOXEL_DATA] = rootVisualElement.Q<VisualElement>("VoxelData");
        }

        void SetMenu(int index)
        {
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i] == null) continue;
                if (i == index)
                    menus[i].style.display = DisplayStyle.Flex;
                else
                    menus[i].style.display = DisplayStyle.None;
            }

            if (index == (int)Menus.VOXEL_DATA)
            {
                // display the voxel data

                // loop through all VoxelData_SO in the Resources folder
                // and display them in the UI
                // allow the user to edit the data

                object[] voxels = Resources.LoadAll(EngineConstants.VOXEL_DATA_PATH);
                foreach (var voxel in voxels)
                {
                    // display the voxel data
                    // allow the user to edit the data

                }
            }
        }

        void SaveMesh(Mesh mesh, string endName = "")
        {
            AssetDatabase.CreateAsset(mesh, "Assets/Minecraft/Meshes/" + previewMesh.meshName + endName + ".asset");
            EditorUtility.SetDirty(previewMesh);
            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            spawnedMesh = GameObject.FindGameObjectWithTag("Mesh Preview");
            string buttonLabel = spawnedMesh ? "Destroy Preview Mesh" : "Preview Mesh";

            Button previewMeshbttn = rootVisualElement.Q<Button>("PreviewMesh");
            if (previewMeshbttn is not null)
                previewMeshbttn.text = buttonLabel;
        }

        void OnSceneGUI(SceneView view)
        {
            MeshCreator.Tick();

            // draw labels for the handles for the enginesettings.debugUI
            Handles.BeginGUI();

            GUILayout.BeginVertical();

            if (DebugSettings.editorShowFPS)
                GUILayout.Label($"FPS: {DebugHelper.FPS}");

            if(PlayerManager.Instance == null)
                PlayerManager.Instance = FindFirstObjectByType<PlayerManager>();

            if (DebugSettings.editorShowActiveChunks)
                GUILayout.Label($"ActiveChunks: {ChunkManager.Chunks.Count}");
            if (DebugSettings.editorShowPlayerChunk)
                GUILayout.Label($"PlayerChunk: {PlayerHelper.PlayerChunk.x}, {PlayerHelper.PlayerChunk.z}");
            if (DebugSettings.editorShowPlayerPos)
                GUILayout.Label($"PlayerPos: {PlayerHelper.PlayerVoxelPosition.x},{PlayerHelper.PlayerVoxelPosition.y}, {PlayerHelper.PlayerVoxelPosition.z}");
            if (DebugSettings.editorShowCameraChunk)
                GUILayout.Label($"CameraChunk: {DebugHelper.CameraChunk.x}, {DebugHelper.CameraChunk.z}");
            if (DebugSettings.editorShowCameraPos)
                GUILayout.Label($"CameraPos: {DebugHelper.CameraPos.x},{DebugHelper.CameraPos.y}, {DebugHelper.CameraPos.z}");
            if (DebugSettings.editorShowCPUTime)
                GUILayout.Label($"CPUTime: {DebugHelper.CPUTime} ms");
            if (DebugSettings.editorShowGPUTime)
                GUILayout.Label($"GPUTime: {DebugHelper.GPUTime} ms");
            if (DebugSettings.editorShowBatches)
                GUILayout.Label($"Batches: {DebugHelper.Batches}");

            GUILayout.EndVertical();

            Handles.EndGUI();

        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }
}