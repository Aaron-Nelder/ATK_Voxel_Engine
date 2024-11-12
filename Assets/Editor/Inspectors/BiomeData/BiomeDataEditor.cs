using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(BiomeData_SO))]
    public class BiomeDataEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        BiomeData_SO _target;

        VisualElement _heightVE;
        VisualElement _caveVE;
        Vector2Field _elevationField;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();

            _target = target as BiomeData_SO;
            _inspectorXML.CloneTree(_inspector);

            Label headerLabel = _inspector.Q<Label>("Header");
            headerLabel.text = _target.name;

            _elevationField = _inspector.Q<Vector2Field>("Elevation");

            // Ensures the elevation field is within the bounds of the chunk size.
            _elevationField.RegisterValueChangedCallback((evt) =>
            {
                float x = Mathf.Clamp(evt.newValue.x, 0, EngineSettings.WorldSettings.ChunkSize.y);
                float y = Mathf.Clamp(evt.newValue.y, 0, EngineSettings.WorldSettings.ChunkSize.y);
                _elevationField.value = new Vector2(x, y);
            });

            NoiseFields();

            // Return the finished Inspector UI.
            return _inspector;
        }

        void NoiseFields()
        {
            ObjectField heightNoiseSO = _inspector.Q<ObjectField>("HeightNoise");
            ObjectField chunkNoiseSO = _inspector.Q<ObjectField>("CaveNoise");
            Foldout noiseFoldout = _inspector.Q<Foldout>("NoiseParameters");

            heightNoiseSO.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null && _heightVE != null)
                    _heightVE.RemoveFromHierarchy();
                else if (evt.newValue is not null)
                {
                    if (_heightVE != null)
                        _heightVE.RemoveFromHierarchy();

                    NoiseProfileEditor heightEditor = CreateInstance<NoiseProfileEditor>();
                    heightEditor.InitElements(_target.HeightNoise);
                    _heightVE = heightEditor.CreateInspectorGUI();
                    _heightVE.Bind(new SerializedObject(_target.HeightNoise));
                    noiseFoldout.Add(_heightVE);
                    _heightVE.style.display = DisplayStyle.Flex;
                }
            });

            chunkNoiseSO.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null && _caveVE != null)
                    _caveVE.RemoveFromHierarchy();
                else if (evt.newValue is not null)
                {
                    if (_caveVE != null)
                        _caveVE.RemoveFromHierarchy();

                    NoiseProfileEditor caveEditor = CreateInstance<NoiseProfileEditor>();
                    caveEditor.InitElements(_target.CaveNoise);
                    _caveVE = caveEditor.CreateInspectorGUI();
                    _caveVE.Bind(new SerializedObject(_target.CaveNoise));
                    noiseFoldout.Add(_caveVE);
                    _caveVE.style.display = DisplayStyle.Flex;
                }
            });
        }
    }
}
