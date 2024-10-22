using UnityEngine.UIElements;
using UnityEditor;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(NoiseProfile_SO))]
    public class NoiseProfileEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        NoiseProfile_SO _target;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();
            _inspectorXML.CloneTree(_inspector);

            try
            {
                InitElements(target as NoiseProfile_SO);
            }
            catch
            {

            }

            if (_target)
            {
                Label name = _inspector.Q<Label>("Header");
                name.text = _target.name;

                // toggle the threshold slider on and off based on the toggle
                Toggle useThreshold = _inspector.Q<Toggle>("Threshold");
                MinMaxSlider threshold = _inspector.Q<MinMaxSlider>("ThresholdSlider");
                Vector2Field thresholdValue = _inspector.Q<Vector2Field>("ThresholdLabel");
                threshold.style.display = _target.UseThreshold ? DisplayStyle.Flex : DisplayStyle.None;
                thresholdValue.style.display = _target.UseThreshold ? DisplayStyle.Flex : DisplayStyle.None;

                useThreshold.RegisterValueChangedCallback((evt) =>
                {
                    threshold.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                    thresholdValue.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                });
            }

            // Return the finished Inspector UI.
            return _inspector;
        }

        public void InitElements(NoiseProfile_SO profile)
        {
            _target = profile;
        }
    }
}