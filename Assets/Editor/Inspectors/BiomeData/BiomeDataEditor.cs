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
        VisualElement _folliageVE;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();

            _target = target as BiomeData_SO;
            _inspectorXML.CloneTree(_inspector);

            Label headerLabel = _inspector.Q<Label>("Header");
            headerLabel.text = _target.name;

            Clamps();
            NoiseFields();

            // Return the finished Inspector UI.
            return _inspector;
        }

        void Clamps()
        {
            FloatField elevationMin = _inspector.Q<FloatField>("elevationMin");
            FloatField elevationMax = _inspector.Q<FloatField>("elevationMax");

            elevationMin.RegisterValueChangedCallback((evt) =>
            {
                //elevationMin.value = Mathf.Clamp(evt.newValue, 0, elevationMax.value);
                //elevationMax.value = Mathf.Clamp(elevationMax.value, elevationMin.value, EngineSettings.WorldSettings.ChunkSize.y);
            });

            _inspector.Q<FloatField>("elevationMax").RegisterValueChangedCallback((evt) =>
            {
                //elevationMin.value = Mathf.Clamp(elevationMin.value, 0, evt.newValue);
                //elevationMax.value = Mathf.Clamp(evt.newValue, elevationMin.value, EngineSettings.WorldSettings.ChunkSize.y);
            });

            FloatField temperatureMin = _inspector.Q<FloatField>("temperatureMin");
            FloatField temperatureMax = _inspector.Q<FloatField>("temperatureMax");

            temperatureMin.RegisterValueChangedCallback((evt) =>
            {
                //temperatureMin.value = Mathf.Clamp(evt.newValue, EngineSettings.WorldSettings.TemperatureRange.x, temperatureMax.value);
                //temperatureMax.value = Mathf.Clamp(temperatureMax.value, temperatureMin.value, EngineSettings.WorldSettings.TemperatureRange.y);
            });

            temperatureMax.RegisterValueChangedCallback((evt) =>
            {
                //temperatureMin.value = Mathf.Clamp(temperatureMin.value, EngineSettings.WorldSettings.TemperatureRange.x, evt.newValue);
                //temperatureMax.value = Mathf.Clamp(evt.newValue, temperatureMin.value, EngineSettings.WorldSettings.TemperatureRange.y);
            });

            float humidityMin = _inspector.Q<FloatField>("humidityMin").value;
            float humidityMax = _inspector.Q<FloatField>("humidityMax").value;

            _inspector.Q<FloatField>("humidityMin").RegisterValueChangedCallback((evt) =>
            {
                //_inspector.Q<FloatField>("humidityMin").value = Mathf.Clamp(evt.newValue, EngineSettings.WorldSettings.HumidityRange.x, humidityMax);
                //_inspector.Q<FloatField>("humidityMax").value = Mathf.Clamp(humidityMax, evt.newValue, EngineSettings.WorldSettings.HumidityRange.y);
            });

            _inspector.Q<FloatField>("humidityMax").RegisterValueChangedCallback((evt) =>
            {
                //_inspector.Q<FloatField>("humidityMin").value = Mathf.Clamp(humidityMin, EngineSettings.WorldSettings.HumidityRange.x, evt.newValue);
                //_inspector.Q<FloatField>("humidityMax").value = Mathf.Clamp(evt.newValue, humidityMin, EngineSettings.WorldSettings.HumidityRange.y);
            });
        }

        void NoiseFields()
        {
            ObjectField heightNoiseSO = _inspector.Q<ObjectField>("HeightNoise");
            ObjectField chunkNoiseSO = _inspector.Q<ObjectField>("CaveNoise");
            ObjectField folliageNoiseSO = _inspector.Q<ObjectField>("FolliageNoise");
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

            folliageNoiseSO.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null && _folliageVE != null)
                    _folliageVE.RemoveFromHierarchy();
                else if (evt.newValue is not null)
                {
                    if (_folliageVE != null)
                        _folliageVE.RemoveFromHierarchy();

                    NoiseProfileEditor folliageEditor = CreateInstance<NoiseProfileEditor>();
                    folliageEditor.InitElements(_target.FolliageNoise);
                    _folliageVE = folliageEditor.CreateInspectorGUI();
                    _folliageVE.Bind(new SerializedObject(_target.FolliageNoise));
                    noiseFoldout.Add(_folliageVE);
                    _folliageVE.style.display = DisplayStyle.Flex;
                }
            });
        }
    }
}
