using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

[CustomEditor(typeof(WorldSettings_SO))]
public class WorldSettingsEditor : Editor
{
    public VisualTreeAsset _inspectorXML;
    VisualElement _inspector;
    WorldSettings_SO _target;
    VisualElement heightNoisePage;

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
        chunkPrefab.objectType = typeof(GameObject);

        ObjectField heightNoiseSO = _inspector.Q<ObjectField>("HeightNoise");
        heightNoiseSO.objectType = typeof(NoiseProfile_SO);

        heightNoisePage = _inspector.Q<VisualElement>("HeightNoisePage");
        heightNoisePage.Q<Label>("Header").text = _target.HeightNoise.name;

        heightNoiseSO.RegisterValueChangedCallback((evt) =>
        {
            if (evt.newValue is null)
                heightNoisePage.style.display = DisplayStyle.None;
            else if (evt.newValue is not null)
            {
                heightNoisePage.Bind(new SerializedObject(_target.HeightNoise));
                heightNoisePage.style.display = DisplayStyle.Flex;
            }
        });

    }
}
