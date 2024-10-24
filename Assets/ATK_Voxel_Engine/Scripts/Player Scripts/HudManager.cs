using UnityEngine;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    public class HudManager : MonoBehaviour
    {
        [SerializeField] UIDocument _hudDocument;
        [SerializeField] DebugHud _debugHUD;

        VisualElement _debugElements;
        VisualElement _staminaBar;

        void Awake()
        {
            new ScreenLogger(_hudDocument);
            AssignElements();
            EnableDebugging(DebugHelper.Debugging);
        }

        void AssignElements()
        {
            _debugElements = _hudDocument.rootVisualElement.Q<VisualElement>("Debug");
            _staminaBar = _hudDocument.rootVisualElement.Q<VisualElement>("Stamina");
            _debugHUD.Init(_hudDocument);
        }

        void OnEnable()
        {
            DebugHelper.OnDebugging += EnableDebugging;
            PlayerManager.Instance.MotionHandler.StamController.OnStaminaChanged += OnStaminaChanged;
        }

        void OnDisable()
        {
            DebugHelper.OnDebugging -= EnableDebugging;
            PlayerManager.Instance.MotionHandler.StamController.OnStaminaChanged -= OnStaminaChanged;
        }

        // enables the debug HUD
        void EnableDebugging(bool enabled)
        {
            if (_debugHUD == null) return;

            if (!_debugHUD.Initialized)

                _debugHUD.enabled = enabled;
            _debugElements.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnStaminaChanged(float curStam, float maxStam)
        {
            _staminaBar.style.backgroundSize = new BackgroundSize(Length.Percent(curStam / maxStam * 100), 100);
        }
    }
}
