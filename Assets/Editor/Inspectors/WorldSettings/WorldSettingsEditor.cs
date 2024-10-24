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
            ObjectField chunkPrefab = _inspector.Q<ObjectField>("ChunkPrefab");
            ObjectField heightNoiseSO = _inspector.Q<ObjectField>("HeightNoise");
            ObjectField chunkNoiseSO = _inspector.Q<ObjectField>("ChunkNoise");

            // Spawn the world settings menu
            NoiseProfileEditor heightEditor = CreateInstance<NoiseProfileEditor>();
            heightEditor.InitElements(_target.HeightNoise);
            _heightVE = heightEditor.CreateInspectorGUI();
            _heightVE.Bind(new SerializedObject(_target.HeightNoise));
            _inspector.Add(_heightVE);

            // Spawn the world settings menu
            NoiseProfileEditor caveEditor = CreateInstance<NoiseProfileEditor>();
            heightEditor.InitElements(_target.CaveNoise);
            VisualElement ce = heightEditor.CreateInspectorGUI();
            ce.Bind(new SerializedObject(_target.CaveNoise));
            _inspector.Add(ce);

            heightNoiseSO.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null)
                    _heightVE.style.display = DisplayStyle.None;
                else if (evt.newValue is not null)
                {
                    _heightVE.Bind(new SerializedObject(_target.HeightNoise));
                    _heightVE.style.display = DisplayStyle.Flex;
                }
            });

            chunkNoiseSO.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null)
                    _caveVE.style.display = DisplayStyle.None;
                else if (evt.newValue is not null)
                {
                    _caveVE.Bind(new SerializedObject(_target.CaveNoise));
                    _caveVE.style.display = DisplayStyle.Flex;
                }
            });
        }
    }
}