using UnityEditor;
using UnityEngine;

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
            AssetDatabase.CreateAsset(mesh, "Assets/Minecraft/Meshes/" + script.meshName + ".asset");
            EditorUtility.SetDirty(script);
            AssetDatabase.SaveAssets();
        }
    }
}
