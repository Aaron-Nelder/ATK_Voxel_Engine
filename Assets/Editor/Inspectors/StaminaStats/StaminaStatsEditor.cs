using UnityEditor;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(StaminaStats_SO))]
    public class StaminaStatsEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        StaminaStats_SO _target;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();

            _target = target as StaminaStats_SO;
            _inspectorXML.CloneTree(_inspector);

            SliderInt s = _inspector.Q<SliderInt>("MaxSlider");
            s.RegisterValueChangedCallback(OnSlider);
            s = _inspector.Q<SliderInt>("StartingSlider");
            s.RegisterValueChangedCallback(OnSlider);

            // Return the finished Inspector UI.
            return _inspector;
        }

        private void OnSlider(ChangeEvent<int> evt)
        {
            // get the maxSlider and set the max value of the slider to the max stamina value
            var maxSlider = _inspector.Q<SliderInt>("MaxSlider");
            if (_target.MaxStamina < _target.Starting)
            {
                maxSlider.lowValue = (int)_target.Starting;
                maxSlider.value = (int)_target.Starting;
            }
            else
            {
                maxSlider.lowValue = 0;
            }

        }
    }
}