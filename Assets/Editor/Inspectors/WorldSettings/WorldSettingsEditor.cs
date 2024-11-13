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

            ObjectField heightNoiseSO = _inspector.Q<ObjectField>("HeightNoise");
            ObjectField chunkNoiseSO = _inspector.Q<ObjectField>("CaveNoise");

            _inspector.Q<UnsignedIntegerField>("XField").RegisterValueChangedCallback(evt =>
            {
                Vector3 newVal = new Vector3(settings.ChunkSize.x, settings.ChunkSize.y, settings.ChunkSize.z);
                _inspector.Q<BoundsField>("ChunkBounds").value = new Bounds(newVal / 2, newVal);
            });

            _inspector.Q<UnsignedIntegerField>("YField").RegisterValueChangedCallback(evt =>
            {
                Vector3 newVal = new Vector3(settings.ChunkSize.x, settings.ChunkSize.y, settings.ChunkSize.z);
                _inspector.Q<BoundsField>("ChunkBounds").value = new Bounds(newVal / 2, newVal);
            });

            _inspector.Q<UnsignedIntegerField>("ZField").RegisterValueChangedCallback(evt =>
            {
                Vector3 newVal = new Vector3(settings.ChunkSize.x, settings.ChunkSize.y, settings.ChunkSize.z);
                _inspector.Q<BoundsField>("ChunkBounds").value = new Bounds(newVal / 2, newVal);
            });

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
                    _inspector.Add(_heightVE);
                    _heightVE.style.display = DisplayStyle.Flex;
                }
            });

            chunkNoiseSO.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null && _caveVE != null)
                {
                    _caveVE.RemoveFromHierarchy();
                }
                else if (evt.newValue is not null)
                {

                    if (_caveVE != null)
                        _caveVE.RemoveFromHierarchy();
                    NoiseProfileEditor caveEditor = CreateInstance<NoiseProfileEditor>();
                    caveEditor.InitElements(_target.CaveNoise);
                    _caveVE = caveEditor.CreateInspectorGUI();
                    _caveVE.Bind(new SerializedObject(_target.CaveNoise));
                    _inspector.Add(_caveVE);
                    _caveVE.style.display = DisplayStyle.Flex;
                }
            });
        }
    }
}