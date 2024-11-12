using UnityEditor;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(MotionStats_SO))]
    public class MotionStatsEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        MotionStats_SO _target;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();

            _target = target as MotionStats_SO;
            _inspectorXML.CloneTree(_inspector);

            // Return the finished Inspector UI.
            return _inspector;
        }
    }
}