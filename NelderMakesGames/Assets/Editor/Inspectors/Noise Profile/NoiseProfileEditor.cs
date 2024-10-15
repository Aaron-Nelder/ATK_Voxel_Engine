using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

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

        _target = target as NoiseProfile_SO;
        _inspectorXML.CloneTree(_inspector);

        Label name = _inspector.Q<Label>("Header");
        name.text = _target.name;

        // Return the finished Inspector UI.
        return _inspector;
    }
}
