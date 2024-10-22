using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    [CustomEditor(typeof(EntityStats_SO))]
    public class EntityStatsEditor : Editor
    {
        public VisualTreeAsset _inspectorXML;
        VisualElement _inspector;
        EntityStats_SO _target;

        VisualElement[] soPage = new VisualElement[2];
        ObjectField[] soFields = new ObjectField[2];

        VisualElement baseStats;

        int pageIndex = 0;
        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our Inspector UI.
            _inspector = new VisualElement();

            _target = target as EntityStats_SO;
            _inspectorXML.CloneTree(_inspector);

            SetupMenuButtons();

            // Return the finished Inspector UI.
            return _inspector;
        }

        void SetupMenuButtons()
        {
            soPage[0] = _inspector.Q<VisualElement>("StaminaStatsEditor");
            soPage[0].Bind(new SerializedObject(_target.StaminaStats));
            soPage[1] = _inspector.Q<VisualElement>("MotionStatsEditor");
            soPage[1].Bind(new SerializedObject(_target.MotionStats));

            soFields[0] = _inspector.Q<ObjectField>("StaminaObject");
            soFields[0].objectType = typeof(StaminaStats_SO);
            soFields[0].RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null)
                    soPage[0].style.display = DisplayStyle.None;
                else if (evt.newValue is not null)
                {
                    soPage[0].Bind(new SerializedObject(_target.StaminaStats));
                    OnToolbarIndex(0);
                }
            });

            ToolbarButton staminaBttn = _inspector.Q<ToolbarButton>("StaminaStatsButton");
            staminaBttn.clickable.clicked += () => { OnToolbarIndex(0); };

            soFields[1] = _inspector.Q<ObjectField>("MotionObject");
            soFields[1].objectType = typeof(MotionStats_SO);
            soFields[1].RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue is null)
                    soPage[1].style.display = DisplayStyle.None;
                else if (evt.newValue is not null)
                {
                    soPage[1].Bind(new SerializedObject(_target.MotionStats));
                    OnToolbarIndex(1);
                }
            });

            ToolbarButton motionBttn = _inspector.Q<ToolbarButton>("MotionStatsButton");
            motionBttn.clickable.clicked += () => { OnToolbarIndex(1); };

            baseStats = _inspector.Q<VisualElement>("BaseStats");
            ToolbarButton baseStatsBttn = _inspector.Q<ToolbarButton>("BaseStatsButton");
            baseStatsBttn.clickable.clicked += () =>
            {
                HideAllPages();
                EnableBaseStats(true);
            };

            HideAllPages();
            OnToolbarIndex(0);
        }

        void HideAllPages()
        {
            foreach (var page in soPage)
                page.style.display = DisplayStyle.None;

            foreach (var so in soFields)
                so.style.display = DisplayStyle.None;

            EnableBaseStats(false);
        }

        void OnToolbarIndex(int newIndex)
        {
            soFields[pageIndex].style.display = DisplayStyle.None;
            soPage[pageIndex].style.display = DisplayStyle.None;

            pageIndex = newIndex;

            soPage[pageIndex].style.display = DisplayStyle.Flex;
            soFields[pageIndex].style.display = DisplayStyle.Flex;

            EnableBaseStats(false);
        }

        void EnableBaseStats(bool enabled) => baseStats.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
    }
}