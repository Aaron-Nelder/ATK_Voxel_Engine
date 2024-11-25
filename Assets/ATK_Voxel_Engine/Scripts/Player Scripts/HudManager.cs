using UnityEngine;
using UnityEngine.UIElements;

namespace ATKVoxelEngine
{
    public class HudManager : MonoBehaviour
    {
        [SerializeField] UIDocument _hudDocument;
        DebugHud _debugHUD;

        VisualElement _debugElements;
        VisualElement _staminaBar;

        bool _initialized;

        public void Initialize()
        {
            _hudDocument.enabled = true;
            new ScreenLogger(_hudDocument);
            AssignElements();
            EnableDebugging(DebugHelper.Debugging);
            _initialized = true;

            DebugHelper.OnDebugging += EnableDebugging;
            PlayerManager.Instance.MotionHandler.StamController.OnStaminaChanged += OnStaminaChanged;
        }

        void AssignElements()
        {
            _debugElements = _hudDocument.rootVisualElement.Q<VisualElement>("Debug");
            _staminaBar = _hudDocument.rootVisualElement.Q<VisualElement>("Stamina");
            if (_debugHUD == null)
                _debugHUD = new DebugHud(_hudDocument);
        }

        void OnEnable()
        {
            if (!_initialized) return;
            DebugHelper.OnDebugging += EnableDebugging;
            PlayerManager.Instance.MotionHandler.StamController.OnStaminaChanged += OnStaminaChanged;
        }

        void OnDisable()
        {
            if (!_initialized) return;
            DebugHelper.OnDebugging -= EnableDebugging;
            PlayerManager.Instance.MotionHandler.StamController.OnStaminaChanged -= OnStaminaChanged;
        }

        // enables the debug HUD
        void EnableDebugging(bool enabled)
        {
            if (_debugHUD == null) return;

            if (enabled && !_debugHUD.Registered)
                _debugHUD.Register();
            else if (_debugHUD.Registered)
                _debugHUD.UnRegister();

            _debugElements.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnStaminaChanged(float curStam, float maxStam)
        {
            _staminaBar.style.backgroundSize = new BackgroundSize(Length.Percent(curStam / maxStam * 100), 100);
        }
    }
}
