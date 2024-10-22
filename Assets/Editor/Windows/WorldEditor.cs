using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    public class WorldEditor : EditorWindow
    {
        WorldSettings_SO WorldSettings => VoxelManager.WorldSettings;
        DebugSettings_SO DebugSettings => VoxelManager.DebugSettings;
        [SerializeField] VisualTreeAsset visualTree = default;

        enum Menus { WorldSettings = 0, DebugUI = 1, Preview = 2, MeshCreator = 3 }
        ToolbarButton[] toolbarButtons = new ToolbarButton[4];
        VisualElement[] menus = new VisualElement[4];

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

            GetMenuButtons();
            GetMenus();
            SetMenu(0);
        }

        void GetMenuButtons()
        {
            toolbarButtons[(int)Menus.WorldSettings] = rootVisualElement.Q<ToolbarButton>("WorldSettingsButton");
            toolbarButtons[(int)Menus.WorldSettings].clicked += () => { SetMenu((int)Menus.WorldSettings); };
            toolbarButtons[(int)Menus.DebugUI] = rootVisualElement.Q<ToolbarButton>("DebugUIButton");
            toolbarButtons[(int)Menus.DebugUI].clicked += () => { SetMenu((int)Menus.DebugUI); };
            toolbarButtons[(int)Menus.Preview] = rootVisualElement.Q<ToolbarButton>("PreviewButton");
            toolbarButtons[(int)Menus.Preview].clicked += () => { SetMenu((int)Menus.Preview); };
            toolbarButtons[(int)Menus.MeshCreator] = rootVisualElement.Q<ToolbarButton>("MeshCreatorButton");
            toolbarButtons[(int)Menus.MeshCreator].clicked += () => { SetMenu((int)Menus.MeshCreator); };
        }

        void GetMenus()
        {
            if (WorldSettings != null)
            {
                // Spawn the world settings menu
                WorldSettingsEditor worldSettingsEditor = ScriptableObject.CreateInstance<WorldSettingsEditor>();
                menus[(int)Menus.WorldSettings] = worldSettingsEditor.CreateInspectorGUI();
                worldSettingsEditor.InitElements(WorldSettings);
                menus[(int)Menus.WorldSettings].Bind(new SerializedObject(WorldSettings));
                rootVisualElement.Add(menus[(int)Menus.WorldSettings]);
            }

            if (DebugSettings != null)
            {
                DebugSettingsEditor debugSettingsEditor = ScriptableObject.CreateInstance<DebugSettingsEditor>();
                menus[(int)Menus.DebugUI] = debugSettingsEditor.CreateInspectorGUI();
                debugSettingsEditor.InitElements(DebugSettings);
                menus[(int)Menus.DebugUI].Bind(new SerializedObject(DebugSettings));
                rootVisualElement.Add(menus[(int)Menus.DebugUI]);
            }

            menus[(int)Menus.Preview] = rootVisualElement.Q<VisualElement>("Preview");
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

            menus[(int)Menus.MeshCreator] = rootVisualElement.Q<VisualElement>("MeshCreator");
        }

        bool subbed = false;
        void SetMenu(int index)
        {
            if (index == (int)Menus.MeshCreator)
            {
                SceneView.duringSceneGui += OnSceneGUI;
                subbed = true;
            }
            else if (subbed)
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                subbed = false;
            }

            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i] == null) continue;
                if (i == index)
                    menus[i].style.display = DisplayStyle.Flex;
                else
                    menus[i].style.display = DisplayStyle.None;


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
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }
}