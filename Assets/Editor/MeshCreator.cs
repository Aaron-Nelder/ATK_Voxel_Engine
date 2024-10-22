using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    public static class MeshCreator
    {
        const float VERTEX_SELECTION_DISTANCE = 100.0F;

        static GUIContent moveIcon;
        static GUIContent rotateIcon;
        static GUIContent scaleIcon;

        static GameObject mObj;
        static MeshFilter mFilter;
        static MeshRenderer mRenderer;
        static CustomMesh_SO mData;

        public static ManipulationType ManipulationType { get; set; }

        static Vector3 sNorm = Vector3.zero;
        static bool useSNorm = false;
        static int sVertIndex = 0;

        static Vector3[] selectedPlane = null;

        static bool selecting = false;

        static Color[] EdgeCols = new Color[]
        {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        };

        static void SetIcons()
        {
            moveIcon = EditorGUIUtility.IconContent("MoveTool");
            rotateIcon = EditorGUIUtility.IconContent("RotateTool");
            scaleIcon = EditorGUIUtility.IconContent("ScaleTool");
        }

        public static GameObject SpawnMesh(CustomMesh_SO meshData, Material mat = null)
        {
            mObj = new GameObject(meshData.meshName);
            mObj.tag = "Mesh Preview";
            mFilter = mObj.AddComponent<MeshFilter>();
            mRenderer = mObj.AddComponent<MeshRenderer>();
            mFilter.sharedMesh = meshData.Mesh;

            if (mat != null)
                mRenderer.sharedMaterial = mat;
            else
                mRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            mData = meshData;
            mObj.transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 5;
            Undo.undoRedoEvent = null;
            Undo.undoRedoEvent += OnUndo;
            ToggleMeshOutline(false);
            SetIcons();
            return mObj;
        }

        static void OnUndo(in UndoRedoInfo info)
        {
            mFilter.sharedMesh = mData.Mesh;
        }

        public static bool DestroyMesh()
        {
            if (mObj is null) return false;
            GameObject.DestroyImmediate(mObj);
            Undo.undoRedoEvent -= OnUndo;
            mObj = null;
            ToggleMeshOutline(true);
            return true;
        }

        public static void Tick()
        {
            if (mObj == null || Undo.isProcessing) return;
            if (Selection.activeGameObject != mObj) return;

            Tools.current = Tool.None;

            WorldAndScreenVerts(mData.vertices, out Vector2[] screenVerts, out Vector3[] worldVerts);

            DrawBoundingBox(worldVerts);

            if (ManipulationType == ManipulationType.Vertex)
                VertexMode(screenVerts, worldVerts);
            else if (ManipulationType == ManipulationType.Plane)
                PlaneMode(worldVerts, screenVerts);
        }

        static void PlaneMode(Vector3[] worldVerts, Vector2[] screenVerts)
        {
            // Check so see if the cursor is close to a plane on the mesh
            PlaceHandles(worldVerts, screenVerts);
        }

        static void VertexMode(Vector2[] screenVerts, Vector3[] worldVerts)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A && !useSNorm)
                useSNorm = true;
            else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.A && useSNorm)
                useSNorm = false;

            DrawVertHandles(mData.vertices, worldVerts, mObj);

            if (!IsMouseCloseToVert(screenVerts, VERTEX_SELECTION_DISTANCE) && !selecting) return;

            GetClosesVertScreen(screenVerts, out int index);
            Vector3 cNorm = mFilter.sharedMesh.normals[index];

            if (Event.current.type == EventType.MouseDown && !selecting)
            {
                selecting = true;
                Undo.RegisterFullObjectHierarchyUndo(mData, "VERT MOVED");
            }
            else if (Event.current.type == EventType.MouseUp && selecting)
                selecting = false;

            // this is here to ensure as a vert is moved, it doesn't switch the selected vert
            sVertIndex = selecting ? sVertIndex : index;
            sNorm = selecting ? sNorm : cNorm;
            sNorm = useSNorm ? sNorm : Vector3.up;

            Vector3 nVertPos = mData.vertices[sVertIndex] + mObj.transform.position;
            nVertPos = Handles.PositionHandle(nVertPos, Quaternion.LookRotation(sNorm));

            mData.vertices[sVertIndex] = nVertPos - mObj.transform.position;
            mFilter.sharedMesh.SetVertices(mData.Vertices);
            mFilter.sharedMesh.SetIndices(mData.Indices, mData.meshTopology, 0);

            mFilter.sharedMesh.SetNormals(mData.Normals);
            mFilter.sharedMesh.RecalculateBounds();
            mFilter.sharedMesh.SetUVs(0, mData.UVS);

            #region GUI Labels
            Handles.BeginGUI();
            Rect startRect = new Rect(10, SceneView.currentDrawingSceneView.position.height - 150, 400, 200);
            EditorGUI.LabelField(startRect, $"Closest Vert: [{index}] {mData.vertices[sVertIndex]}");
            startRect.y -= 20;
            EditorGUI.LabelField(startRect, $"Cursor Position: {Event.current.mousePosition}");
            Handles.EndGUI();
            #endregion
        }

        static Vector2 GetClosesVertScreen(Vector2[] verts, out int index)
        {
            Vector2 mousePos = Event.current.mousePosition;
            Vector2 cVert = Vector2.zero;
            float minDist = float.MaxValue;
            index = 0;
            for (int i = 0; i < verts.Length; i++)
            {
                float dist = Vector2.Distance(mousePos, verts[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    cVert = verts[i];
                    index = i;
                }
            }

            return cVert;
        }

        static void PlaceHandles(Vector3[] worldVerts, Vector2[] screenVerts)
        {
            int offset = mData.meshTopology == MeshTopology.Triangles ? 3 : 4;

            // loops through each triangle or quad
            for (int i = 0; i < mData.vertices.Length; i += offset)
            {
                Vector3 center = (worldVerts[i] + worldVerts[i + 1] + worldVerts[i + 2] + worldVerts[i + 3]) / 4;
                Vector2 screenCenter = (screenVerts[i] + screenVerts[i + 1] + screenVerts[i + 2] + screenVerts[i + 3]) / 4;

                // checks to see if the mouse cursor is close to the center of the plane
                float dist = Vector2.Distance(Event.current.mousePosition, screenCenter);

                if (dist > VERTEX_SELECTION_DISTANCE) continue;

                Debug.Log($"Distance: {dist}");

                if (Event.current.type == EventType.MouseDown && selectedPlane == null)
                {
                    selectedPlane = new Vector3[] { mData.vertices[i], mData.vertices[i + 1], mData.vertices[i + 2], mData.vertices[i + 3] };
                    Debug.Log($"Selected: {mData.vertices[i]},{mData.vertices[i + 1]},{mData.vertices[i + 2]},{mData.vertices[i + 3]}");
                    Undo.RegisterFullObjectHierarchyUndo(mData, "VERT MOVED");
                }
                else if (Event.current.type == EventType.MouseUp && selectedPlane != null)
                    selectedPlane = null;

                if (selectedPlane == null) continue;

                // loops through each vert in the plane to see if the selected vert is in the plane
                bool containsVert = false;
                for (int j = 0; j < 4; j++)
                {
                    if (selectedPlane[j] == mData.vertices[i + j])
                    {
                        containsVert = true;
                        break;
                    }
                }

                if (!containsVert) continue;

                // Draws the edges of the plane in the scene view
                DrawEdgeForPlane(new Vector3[] { worldVerts[i], worldVerts[i + 1], worldVerts[i + 2], worldVerts[i + 3] });

                // draw the position handle for the center of the plane
                Vector3 nCenter = Handles.PositionHandle(center, Quaternion.identity);
                Vector3 o = nCenter - center;

                // check to see if the handle has been selected
                if (o != Vector3.zero)
                {
                    // move the plane based on the drag
                    Vector3[] verts = new Vector3[] { worldVerts[i], worldVerts[i + 1], worldVerts[i + 2], worldVerts[i + 3] };
                    for (int j = 0; j < verts.Length; j++)
                    {
                        Vector3 vert = verts[j];
                        Vector3 nVert = vert + o;
                        verts[j] = nVert;
                    }
                    // set the new vertices
                    mData.vertices[i] = verts[0] - mObj.transform.position;
                    mData.vertices[i + 1] = verts[1] - mObj.transform.position;
                    mData.vertices[i + 2] = verts[2] - mObj.transform.position;
                    mData.vertices[i + 3] = verts[3] - mObj.transform.position;
                }
            }

            mFilter.sharedMesh.vertices = mData.vertices;
            mFilter.sharedMesh.RecalculateBounds();
            mFilter.sharedMesh.RecalculateNormals();
        }

        static bool IsMouseCloseToVert(Vector2[] verts, float distance)
        {
            Vector2 mousePos = Event.current.mousePosition;
            for (int i = 0; i < verts.Length; i++)
            {
                float dist = Vector2.Distance(mousePos, verts[i]);
                if (dist < distance)
                    return true;
            }

            return false;
        }

        static void DrawEdgeForPlane(Vector3[] verts)
        {
            Handles.color = Color.red;
            Handles.DrawLines(new Vector3[] { verts[0], verts[1], verts[1], verts[2], verts[2], verts[3], verts[3], verts[0] });
        }

        // Draws the vert lines in the scene view
        static void DrawEdgeCols(CustomMesh_SO mdata, Vector3[] verts)
        {
            int inc = mdata.meshTopology == MeshTopology.Triangles ? 3 : 4;
            Handles.color = Color.red;
            for (int i = 0; i < mdata.vertices.Length; i += inc)
            {
                Handles.color = EdgeCols[i % EdgeCols.Length];
                if (inc > 3)
                {
                    Vector3 a = verts[i];
                    Vector3 b = verts[i + 1];
                    Vector3 c = verts[i + 2];
                    Vector3 d = verts[i + 3];
                    Handles.DrawLines(new Vector3[] { a, b, b, c, c, d, d, a });
                }
                else
                {
                    Vector3 a = verts[i];
                    Vector3 b = verts[i + 1];
                    Vector3 c = verts[i + 2];
                    Handles.DrawLines(new Vector3[] { a, b, b, c, c, a });
                }
            }
        }

        static void DrawVertHandles(Vector3[] localVerts, Vector3[] worldVerts, GameObject obj)
        {
            GUIStyle inBoundsStyle = new GUIStyle { normal = new GUIStyleState { textColor = Color.green } };
            GUIStyle outBoundsStyle = new GUIStyle { normal = new GUIStyleState { textColor = Color.red } };
            Vector3 avg = Vector3.zero;

            foreach (var vert in worldVerts)
                avg += vert;
            avg /= worldVerts.Length;

            for (int i = 0; i < localVerts.Length; i++)
            {
                Vector3 dirFromAvg = (avg - localVerts[i]).normalized;

                bool inBounds = true;
                if ((Mathf.Abs(localVerts[i].x) > 0.5f) || (Mathf.Abs(localVerts[i].y) > 0.5f) || (Mathf.Abs(localVerts[i].z) > 0.5f))
                    inBounds = false;

                if (i == sVertIndex && selecting)
                    Handles.color = Color.blue;
                else
                    Handles.color = inBounds ? Color.green : Color.red;

                Handles.SphereHandleCap(0, worldVerts[i], Quaternion.identity, 0.1f, EventType.Repaint);

                float dist = Vector3.Distance(localVerts[i], SceneView.lastActiveSceneView.camera.transform.position);


                GUIStyle style = inBounds ? inBoundsStyle : outBoundsStyle;
                string label = $"[{i}] {localVerts[i]}";
                Handles.Label(worldVerts[i] - (dirFromAvg * 0.1f), label, style);
            }
        }

        static void ToggleMeshOutline(bool enabled)
        {
            var annotationUtility = typeof(Editor).Assembly.GetType("UnityEditor.AnnotationUtility");
            var showSelectionOutline = annotationUtility.GetProperty("showSelectionOutline", BindingFlags.NonPublic | BindingFlags.Static);
            showSelectionOutline.SetValue(null, enabled);
        }

        static void DrawBoundingBox(Vector3[] worldVerts)
        {
            Handles.color = Color.cyan;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.DrawWireCube(mObj.transform.position, Vector3.one);
        }

        static void WorldAndScreenVerts(Vector3[] verts, out Vector2[] screenVerts, out Vector3[] worldVerts)
        {
            int len = verts.Length;
            screenVerts = new Vector2[len];
            worldVerts = new Vector3[len];
            for (int i = 0; i < len; i++)
            {
                worldVerts[i] = mObj.transform.rotation * verts[i] + mObj.transform.position;
                screenVerts[i] = HandleUtility.WorldToGUIPoint(worldVerts[i]);
            }
        }
    }

    #region Scene View Overlay
    [Overlay(typeof(SceneView), "Mesh Creator Tool Bar")]
    public class CustomToolbarOverlay : ToolbarOverlay
    {
        public CustomToolbarOverlay() : base(VertexToggle.ID, PlaneToggle.ID, ScaleToggle.ID)
        {
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    class VertexToggle : EditorToolbarToggle
    {
        public const string ID = "MeshCreator/VertexTool";
        public static VertexToggle Instance;

        public VertexToggle()
        {
            icon = EditorGUIUtility.IconContent("LightProbeProxyVolume Gizmo").image as Texture2D;
            tooltip = "Vertex";
            value = MeshCreator.ManipulationType == ManipulationType.Vertex;

            Instance = this;

            // Add the onValueChanged event listener
            this.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue) // Check if the value is true or false
                    MeshCreator.ManipulationType = ManipulationType.Vertex;

                // Set the other toggles to false
                PlaneToggle.Instance.SetValueWithoutNotify(false);
                ScaleToggle.Instance.SetValueWithoutNotify(false);
            });
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    class PlaneToggle : EditorToolbarToggle
    {
        public const string ID = "MeshCreator/PlaneTool";
        public static PlaneToggle Instance;

        public PlaneToggle()
        {
            icon = EditorGUIUtility.IconContent("d_ViewOptions").image as Texture2D;
            tooltip = "Plane";
            value = MeshCreator.ManipulationType == ManipulationType.Plane;

            Instance = this;

            // Add the onValueChanged event listener
            this.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue) // Check if the value is true or false
                    MeshCreator.ManipulationType = ManipulationType.Plane;

                // Set the other toggles to false
                VertexToggle.Instance.SetValueWithoutNotify(false);
                ScaleToggle.Instance.SetValueWithoutNotify(false);
            });
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    class ScaleToggle : EditorToolbarToggle
    {
        public const string ID = "MeshCreator/ScaleTool";
        public static ScaleToggle Instance;

        public ScaleToggle()
        {
            icon = EditorGUIUtility.IconContent("ScaleTool").image as Texture2D;
            tooltip = "Scale";
            value = MeshCreator.ManipulationType == ManipulationType.Scale;

            Instance = this;

            // Add the onValueChanged event listener
            this.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue) // Check if the value is true or false
                    MeshCreator.ManipulationType = ManipulationType.Scale;

                // Set the other toggles to false
                VertexToggle.Instance.SetValueWithoutNotify(false);
                PlaneToggle.Instance.SetValueWithoutNotify(false);
            });
        }
    }

    #endregion

    public enum ManipulationType
    {
        Vertex,
        Plane,
        Scale
    }
}