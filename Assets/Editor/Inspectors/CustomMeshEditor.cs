using UnityEditor;
using UnityEngine;


namespace ATKVoxelEngine
{
    [CustomEditor(typeof(CustomMesh_SO))]
    public class CustomMeshEditor : Editor
    {
        CustomMesh_SO script;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            script = (CustomMesh_SO)target;
            if (GUILayout.Button("Save Mesh"))
            {
                Mesh mesh = script.SaveMesh();
                AssetDatabase.CreateAsset(mesh, "Assets/ATK_Voxel_Engine/Meshes/" + script.meshName + "_Mesh" + ".asset");
                EditorUtility.SetDirty(script);
                AssetDatabase.SaveAssets();
            }
        }
    }
}