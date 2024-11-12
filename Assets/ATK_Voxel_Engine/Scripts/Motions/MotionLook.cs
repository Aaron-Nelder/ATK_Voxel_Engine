using UnityEngine;
using UnityEngine.InputSystem;

namespace ATKVoxelEngine
{
    public class MotionLook : BaseMotion
    {
        public override bool IsEnabled { get; protected set; } = false;
        public override MotionType Type => MotionType.LOOK;
        public override TickType TickType => TickType.UPDATE;
        public override EntityMotionHandler Handler { get; protected set; }

        [Header("Cursor")]
        [SerializeField] CursorLockMode _CstartM = CursorLockMode.Locked;
        bool _fLook = false;

        [Header("Sensitivity")]
        [SerializeField] Vector2 _sensitivity = new Vector2(5, 5);
        [SerializeField] Vector2 _vClamp = new Vector2(-90, 90);
        [SerializeField] Vector2 _smoothing = new(5, 5);

        Vector2 _input = new Vector2();
        float _xRot = 0.0f;     // Vertical rotation
        float _yRot = 0.0f;     // Horizontal rotation
        float _xCurRot = 0.0f;  // Smoothed vertical rotation
        float _xRotVel = 0.0f;  // Velocity for smooth damp
        float _yCurRot = 0.0f;  // Smoothed horizontal rotation
        float _yRotVel = 0.0f;  // Velocity for smooth damp

        void OnLook(InputValue value) => _input = value.Get<Vector2>();

        void OnDebugCam()
        {
            _fLook = !_fLook;
            Cursor.lockState = _fLook ? CursorLockMode.None : _CstartM;
        }

        public override void Tick(float deltaTime)
        {
            if (_fLook) return;

            _input *= _sensitivity * deltaTime;

            ProcXRot(_input.y);
            ProcYRot(_input.x);
        }

        // Apply vertical rotation to the camera
        void ProcXRot(float yInput, bool useSmoothing = true)
        {
            _xRot = Mathf.Clamp(_xRot - yInput, -90f, 90f);
            _xCurRot = Mathf.SmoothDamp(_xCurRot, _xRot, ref _xRotVel, useSmoothing ? _smoothing.y : 0);
            Handler.Head.localRotation = Quaternion.Euler(_xCurRot, 0f, 0f);
        }

        // Apply horizontal rotation to the player character controller
        void ProcYRot(float xInput, bool useSmoothing = true)
        {
            _yRot += xInput;
            _yCurRot = Mathf.SmoothDamp(_yCurRot, _yRot, ref _yRotVel, useSmoothing ? _smoothing.x : 0);
            Handler.Body.transform.rotation = Quaternion.Euler(0f, _yCurRot, 0f);
        }

        protected override void ResetMotion()
        {
            base.ResetMotion();
            _smoothing = _smoothing * 0.01f;
            Cursor.lockState = _CstartM;
        }
    }
}
