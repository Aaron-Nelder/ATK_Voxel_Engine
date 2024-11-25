using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(WorldSettings_SO))]
    public class WorldSettingsEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        WorldSettings_SO _target;

        VisualElement _heightVE;
        VisualElement _caveVE;

        VisualElement _humidityVE;
        VisualElement _temperatureVE;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();
            _inspectorXML.CloneTree(_inspector);

            try
            {
                InitElements(target as WorldSettings_SO);
            }
            catch { return _inspector; }

            // Return the finished Inspector UI.
            return _inspector;
        }

        public void InitElements(WorldSettings_SO settings)
        {
            if (settings is null)
            {
                Debug.LogWarning("Target is null");
                return;
            }

            _target = settings;

            ChunkSizeChecks();

            RegisterNoise();
        }

        void RegisterNoise()
        {
            _inspector.Q<ObjectField>("HumidityNoise").RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null && _humidityVE != null)
                    _humidityVE.RemoveFromHierarchy();
                else if (evt.newValue is not null)
                {
                    if (_humidityVE != null)
                        _humidityVE.RemoveFromHierarchy();
                    NoiseProfileEditor heightEditor = CreateInstance<NoiseProfileEditor>();
                    heightEditor.InitElements(_target.HumidityNoise);
                    _humidityVE = heightEditor.CreateInspectorGUI();
                    _humidityVE.Bind(new SerializedObject(_target.HumidityNoise));
                    _inspector.Add(_humidityVE);
                    _humidityVE.style.display = DisplayStyle.Flex;
                }
            });

            _inspector.Q<ObjectField>("TemperatureNoise").RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null && _temperatureVE != null)
                    _temperatureVE.RemoveFromHierarchy();
                else if (evt.newValue is not null)
                {
                    if (_temperatureVE != null)
                        _temperatureVE.RemoveFromHierarchy();
                    NoiseProfileEditor heightEditor = CreateInstance<NoiseProfileEditor>();
                    heightEditor.InitElements(_target.TemperatureNoise);
                    _temperatureVE = heightEditor.CreateInspectorGUI();
                    _temperatureVE.Bind(new SerializedObject(_target.TemperatureNoise));
                    _inspector.Add(_temperatureVE);
                    _temperatureVE.style.display = DisplayStyle.Flex;
                }
            });
        }

        void ChunkSizeChecks()
        {
            _inspector.Q<UnsignedIntegerField>("XField").RegisterValueChangedCallback(evt =>
            {
                Vector3 newVal = new Vector3(_target.ChunkSize.x, _target.ChunkSize.y, _target.ChunkSize.z);
                _inspector.Q<BoundsField>("ChunkBounds").value = new Bounds(newVal / 2, newVal);
            });

            _inspector.Q<UnsignedIntegerField>("YField").RegisterValueChangedCallback(evt =>
            {
                Vector3 newVal = new Vector3(_target.ChunkSize.x, _target.ChunkSize.y, _target.ChunkSize.z);
                _inspector.Q<BoundsField>("ChunkBounds").value = new Bounds(newVal / 2, newVal);
            });

            _inspector.Q<UnsignedIntegerField>("ZField").RegisterValueChangedCallback(evt =>
            {
                Vector3 newVal = new Vector3(_target.ChunkSize.x, _target.ChunkSize.y, _target.ChunkSize.z);
                _inspector.Q<BoundsField>("ChunkBounds").value = new Bounds(newVal / 2, newVal);
            });
        }
    }
}