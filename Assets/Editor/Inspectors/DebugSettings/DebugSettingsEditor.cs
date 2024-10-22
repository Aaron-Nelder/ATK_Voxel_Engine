using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(DebugSettings_SO))]
    public class DebugSettingsEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        DebugSettings_SO _target;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();
            _inspectorXML.CloneTree(_inspector);

            try
            {
                InitElements(target as DebugSettings_SO);
            }
            catch { return _inspector; }


            // Return the finished Inspector UI.
            return _inspector;
        }

        public void InitElements(DebugSettings_SO settings)
        {
            if (settings is null)
            {
                Debug.LogWarning("Target is null");
                return;
            }

            _target = settings;
        }
    }
}