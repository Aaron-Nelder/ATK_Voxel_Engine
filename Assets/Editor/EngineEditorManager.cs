using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace ATKVoxelEngine.EditorManager
{
    public enum EditorMode { DEFAULT, IN_GAME, MESH_EDITOR }

    [InitializeOnLoad]
    public class EngineEditorManager
    {
        public static EditorMode EditorMode { get; private set; }

        // Settings
        public static WorldSettings_SO WorldSettings => EngineSettings.WorldSettings;
        public static DebugSettings_SO DebugSettings => EngineSettings.DebugSettings;

        // GUI Styles
        static GUIStyle _defaultLabel;
        static GUIStyle _defaultHeader;
        static GUIStyle _inGameHeader;
        static GUIStyle _meshEditorHeader;

        //Mesh Editor
        static GameObject _customMeshOBJ;
        static Mesh _customMesh;
        static VoxelMeshData_SO _meshData;
        static int _selectedPlaneIndex = -1;

        static EngineEditorManager()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            SceneView.duringSceneGui += Tick;

            if (PlayerManager.Instance == null)
                PlayerManager.Instance = GameObject.FindFirstObjectByType<PlayerManager>();

            SetGUIStyles();
        }

        #region Tick
        static void Tick(SceneView sceneView)
        {
            switch (EditorMode)
            {
                case EditorMode.IN_GAME:
                    InGameTick();
                    break;
                case EditorMode.MESH_EDITOR:
                    MeshEditorTick();
                    break;
                default:
                    DefaultTick();
                    break;
            }
        }
        static void DefaultTick() { }
        static void InGameTick() { }
        static void MeshEditorTick()
        {
            _customMeshOBJ = GameObject.FindGameObjectWithTag(EngineConstants.CUSTOM_MESH_TAG);
            _meshData = _customMeshOBJ?.GetComponent<CustomMeshReference>().MeshData;
            _customMesh = _customMeshOBJ?.GetComponent<MeshFilter>().sharedMesh;

            // set the _customMeshOBJ as unselectable
            if (_customMeshOBJ != null)
                _customMeshOBJ.hideFlags = HideFlags.NotEditable;

            // return to default mode if no custom mesh is found
            if (_customMeshOBJ is null)
            {
                _selectedPlaneIndex = -1;
                SetEditorMode(EditorMode.DEFAULT);
                return;
            }

            float3 objPos = new(_customMeshOBJ.transform.position.x, _customMeshOBJ.transform.position.y, _customMeshOBJ.transform.position.z);

            bool isHovered = false;
            int hoverIndex = -1;

            // If there isn't a selected plane
            if (_selectedPlaneIndex == -1)
            {
                // loops through all planes
                for (int i = 0; i < _meshData.MeshPlanes.Length; i++)
                {
                    float3 floatPos = new();

                    // Gets the average position of the verticies
                    for (int j = 0; j < MeshPlane.VERTEX_COUNT; j++)
                        floatPos += _meshData.MeshPlanes[i].Vertices[j].position;
                    floatPos /= MeshPlane.VERTEX_COUNT;
                    floatPos += objPos;

                    Vector3 vecPos = new(floatPos.x, floatPos.y, floatPos.z);
                    Quaternion rotation = _customMeshOBJ.transform.rotation;

                    Vector3 screenPosition = WorldPosToScreenPos(vecPos);
                    Vector3 mousePosition = Event.current.mousePosition;
                    Handles.color = Color.red;

                    // checks to see if the mouse is hovering over the plane center
                    if (Vector3.Distance(screenPosition, mousePosition) < 25)
                    {
                        Handles.color = Color.green;
                        hoverIndex = i;
                        isHovered = true;
                    }

                    Handles.SphereHandleCap(0, vecPos, rotation, 0.1f, EventType.Repaint);
                }
            }
            else
            {
                float3 floatPos = new();

                // Gets the average position of the verticies
                for (int i = 0; i < MeshPlane.VERTEX_COUNT; i++)
                    floatPos += _meshData.MeshPlanes[_selectedPlaneIndex].Vertices[i].position;
                floatPos /= MeshPlane.VERTEX_COUNT;
                floatPos += objPos;

                Vector3 vecPos = new(floatPos.x, floatPos.y, floatPos.z);
                Quaternion rotation = _customMeshOBJ.transform.rotation;

                EditorGUI.BeginChangeCheck();
                Vector3 newVecPos = Handles.PositionHandle(vecPos, rotation);

                if (EditorGUI.EndChangeCheck())
                {
                    newVecPos -= vecPos;
                    // update the position of the vertices
                    for (int i = 0; i < MeshPlane.VERTEX_COUNT; i++)
                        _meshData.MeshPlanes[_selectedPlaneIndex].Vertices[i].position += new float3(newVecPos.x, newVecPos.y, newVecPos.z);

                    // save change to undo stack
                    Undo.RecordObject(_meshData, "Change Plane Position");
                    UpdateMesh();
                }
            }

            if (isHovered && Event.current.type == EventType.MouseUp && Event.current.button == 0)
                _selectedPlaneIndex = hoverIndex;
            else if (!isHovered && Event.current.type == EventType.MouseUp && Event.current.button == 0)
                _selectedPlaneIndex = -1;

            /*
            EditorGUI.BeginChangeCheck();
            Vector3 newVecPos = Handles.PositionHandle(vecPos, rotation);

            if (EditorGUI.EndChangeCheck())
            {
                newVecPos -= vecPos;
                // update the position of the vertices
                for (int j = 0; j < _meshData.MeshPlanes[i].Vertices.Length; j++)
                    _meshData.MeshPlanes[i].Vertices[j].position += new float3(newVecPos.x, newVecPos.y, newVecPos.z);

                // update the mesh
                _meshData.SaveMesh();

                // update the mesh object
                _customMeshOBJ.GetComponent<MeshFilter>().mesh = _meshData.Mesh;
            }
            */
        }

        static void UpdateMesh()
        {
            NativeArray<float3> Vertices = new NativeArray<float3>(_meshData.MeshPlanes.Length * 4, Allocator.Temp);
            NativeArray<uint> Indices = new NativeArray<uint>(_meshData.MeshPlanes.Length * 6, Allocator.Temp);
            NativeArray<float3> Normals = new NativeArray<float3>(_meshData.MeshPlanes.Length * 4, Allocator.Temp);
            NativeArray<float2> UVS = new NativeArray<float2>(_meshData.MeshPlanes.Length * 4, Allocator.Temp);

            uint vOffset = 0;
            for (int i = 0; i < _meshData.MeshPlanes.Length; i++)
            {
                for (int j = 0; j < MeshPlane.INDEX_COUNT; j++)
                    Indices[i * MeshPlane.INDEX_COUNT + j] = MeshPlane.Indices[j] + vOffset;

                vOffset += MeshPlane.VERTEX_COUNT;

                for (int j = 0; j < MeshPlane.VERTEX_COUNT; j++)
                {
                    Vertices[i * MeshPlane.VERTEX_COUNT + j] = _meshData.MeshPlanes[i].Vertices[j].position;
                    Normals[i * MeshPlane.VERTEX_COUNT + j] = _meshData.MeshPlanes[i].Vertices[j].normal;
                    UVS[i * MeshPlane.VERTEX_COUNT + j] = _meshData.MeshPlanes[i].texture.uv[j];
                }
            }

            _customMesh.SetVertices(Vertices);
            _customMesh.SetIndices(Indices, MeshTopology.Triangles, 0);
            _customMesh.SetNormals(Normals);
            _customMesh.SetUVs(0, UVS);

            _customMesh.RecalculateBounds();
            _customMesh.RecalculateTangents();

            Vertices.Dispose();
            Indices.Dispose();
            Normals.Dispose();
            UVS.Dispose();
        }

        static Vector3 WorldPosToScreenPos(Vector3 worldPos)
        {
            return HandleUtility.WorldToGUIPoint(worldPos);
        }
        #endregion

        #region SceneGUI      
        // Draws the scene GUI labels
        static void OnSceneGUI(SceneView sceneView)
        {
            Handles.BeginGUI();

            EditorGUI.LabelField(new(10, 10, 200, 200), $"EditorMode: {EditorMode.ToString()}", GetHeaderStyle(EditorMode));

            switch (EditorMode)
            {
                case EditorMode.MESH_EDITOR:
                    MeshEditorGUI();
                    break;
                default:
                    DefaultGUI();
                    break;
            }
            Handles.EndGUI();
        }
        static void DefaultGUI()
        {
            if (PlayerManager.Instance == null)
                PlayerManager.Instance = GameObject.FindFirstObjectByType<PlayerManager>();

            Rect rect = new(10, -50, 200, 200);
            if (DebugSettings.editorShowFPS)
            {
                EditorGUI.LabelField(rect, $"FPS: {DebugHelper.FPS}", _defaultLabel);
                rect.y += 20;
            }

            if (DebugSettings.editorShowActiveChunks)
            {
                EditorGUI.LabelField(rect, $"ActiveChunks: {ChunkManager.Chunks.Count}", _defaultLabel);
                rect.y += 20;
            }
            if (DebugSettings.editorShowPlayerChunk)
            {
                EditorGUI.LabelField(rect, $"PlayerChunk: {PlayerHelper.PlayerChunk.x}, {PlayerHelper.PlayerChunk.z}", _defaultLabel);
                rect.y += 20;
            }
            if (DebugSettings.editorShowPlayerPos)
            {
                EditorGUI.LabelField(rect, $"PlayerPos: {PlayerHelper.PlayerVoxelPosition.x},{PlayerHelper.PlayerVoxelPosition.y}, {PlayerHelper.PlayerVoxelPosition.z}", _defaultLabel);
                rect.y += 20;
            }
            if (DebugSettings.editorShowCameraChunk)
            {
                EditorGUI.LabelField(rect, $"CameraChunk: {DebugHelper.CameraChunk.x}, {DebugHelper.CameraChunk.z}", _defaultLabel);
                rect.y += 20;
            }
            if (DebugSettings.editorShowCameraPos)
            {
                EditorGUI.LabelField(rect, $"CameraPos: {DebugHelper.CameraPos.x},{DebugHelper.CameraPos.y}, {DebugHelper.CameraPos.z}", _defaultLabel);
                rect.y += 20;
            }
            if (DebugSettings.editorShowCPUTime)
            {
                EditorGUI.LabelField(rect, $"CPUTime: {DebugHelper.CPUTime} ms", _defaultLabel);
                rect.y += 20;
            }
            if (DebugSettings.editorShowGPUTime)
            {
                EditorGUI.LabelField(rect, $"GPUTime: {DebugHelper.GPUTime} ms", _defaultLabel);
                rect.y += 20;
            }
            if (DebugSettings.editorShowBatches) EditorGUI.LabelField(rect, $"Batches: {DebugHelper.Batches}", _defaultLabel);
        }
        static void MeshEditorGUI()
        {
            if (_customMeshOBJ == null) return;

            if (_selectedPlaneIndex == -1)
            {
                Rect rect = new(10, -50, 200, 200);
                EditorGUI.LabelField(rect, $"Name: {_customMeshOBJ.name}", _defaultLabel);
                rect.y += 20;
                EditorGUI.LabelField(rect, $"Vertices: {_meshData.Mesh.vertexCount}", _defaultLabel);
                rect.y += 20;
                EditorGUI.LabelField(rect, $"Triangles: {_meshData.Mesh.triangles.Length / 3}", _defaultLabel);
                rect.y += 20;
                EditorGUI.LabelField(rect, $"Material: {_meshData.Material.name}", _defaultLabel);
            }
            else
            {
                Rect rect = new(10, 25, 200, 50);
                EditorGUI.LabelField(rect, $"Plane[{_selectedPlaneIndex}]", _defaultLabel);
                rect.y += 20;
                for (int i = 0; i < MeshPlane.VERTEX_COUNT; i++)
                {
                    EditorGUI.LabelField(rect, $"Vertex[{i}]", _defaultLabel);
                    rect.y += 35;
                    Vector3 pos = EditorGUI.Vector3Field(rect, "  Position", _meshData.MeshPlanes[_selectedPlaneIndex].Vertices[i].position);
                    rect.y += 40;
                    Vector3 norm = EditorGUI.Vector3Field(rect, "  Normal", _meshData.MeshPlanes[_selectedPlaneIndex].Vertices[i].normal);
                    rect.y += 40;
                    Vector2 uv = EditorGUI.Vector2Field(rect, "  UV", _meshData.MeshPlanes[_selectedPlaneIndex].Vertices[i].texCoord0);
                    rect.y -= 10;
                    if (i != MeshPlane.VERTEX_COUNT - 1)
                        rect.y += 40;

                    if (!new float3(pos.x, pos.y, pos.z).Equals(_meshData.MeshPlanes[_selectedPlaneIndex].Vertices[i].position))
                        UpdateMesh();
                    else if (!new float3(norm.x, norm.y, norm.z).Equals(_meshData.MeshPlanes[_selectedPlaneIndex].Vertices[i].normal))
                        UpdateMesh();
                    else if (!new float2(uv.x, uv.y).Equals(_meshData.MeshPlanes[_selectedPlaneIndex].Vertices[i].texCoord0))
                        UpdateMesh();

                    _meshData.MeshPlanes[_selectedPlaneIndex].Vertices[i] = new Vertex(new float3(pos.x, pos.y, pos.z), new float3(norm.x, norm.y, norm.z), new float2(uv.x, uv.y));
                }
            }
        }
        static GUIStyle GetHeaderStyle(EditorMode mode)
        {
            switch (mode)
            {
                case EditorMode.DEFAULT:
                    return _defaultHeader;
                case EditorMode.IN_GAME:
                    return _inGameHeader;
                case EditorMode.MESH_EDITOR:
                    return _meshEditorHeader;
                default:
                    return _defaultLabel;
            }
        }
        static void SetGUIStyles()
        {
            _defaultLabel = new GUIStyle();
            _defaultLabel.fontSize = 16;
            _defaultLabel.normal.textColor = Color.white;
            _defaultLabel.alignment = TextAnchor.MiddleLeft;

            _defaultHeader = new GUIStyle();
            _defaultHeader.fontSize = 18;
            _defaultHeader.normal.textColor = Color.white;
            _defaultHeader.fontStyle = FontStyle.BoldAndItalic;

            _inGameHeader = new GUIStyle();
            _inGameHeader.fontSize = 18;
            _inGameHeader.normal.textColor = Color.green;
            _inGameHeader.fontStyle = FontStyle.BoldAndItalic;

            _meshEditorHeader = new GUIStyle();
            _meshEditorHeader.fontSize = 18;
            _meshEditorHeader.normal.textColor = Color.yellow;
            _meshEditorHeader.fontStyle = FontStyle.BoldAndItalic;
        }
        #endregion

        public static void SetEditorMode(EditorMode mode)
        {
            _selectedPlaneIndex = -1;
            EditorMode = mode;
        }
    }
}