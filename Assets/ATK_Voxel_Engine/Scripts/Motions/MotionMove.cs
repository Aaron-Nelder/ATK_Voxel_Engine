using UnityEngine;
using UnityEngine.InputSystem;

public class MotionMove : BaseMotion
{
    public override bool IsEnabled { get; protected set; } = false;
    public override MotionType Type { get; protected set; } = MotionType.Moving;
    public override EntityMotionHandler Handler { get; protected set; }

    [Header("Walk Settings")]
    [SerializeField] float[] _wClampX = new float[] { 4.0f, 4.0f };
    [SerializeField] float[] _wClampZ = new float[] { 2.5f, 4.0f };

    [Header("Sprint Settings")]
    [SerializeField] float[] _sClampX = new float[] { 6.0f, 6.0f };
    [SerializeField] float[] _sClampZ = new float[] { 4.5f, 6.0f };
    [SerializeField] float _stamCostPerSec = 10.0f;

    [Header("Acceleration / Deceleration")]
    [SerializeField] float _startAccel = 0.5f;
    [SerializeField] float _accelPerSec = 2.0f;
    [SerializeField] float _decelPerSec = 2.0f;
    [SerializeField] float _resetAccelAng = 45.0f;

    Vector3 _moveInput = new();
    Vector3 _lastMoveInput = new();
    float _curAccel = 0;
    bool _sprintInput = false;

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        _moveInput = new(input.x, 0, input.y);
    }

    void OnSprint(InputValue value) => _sprintInput = value.isPressed;

    public override void ProccessUpdate()
    {
        base.ProccessUpdate();

        if (_moveInput.Equals(Vector3.zero))
            _curAccel -= _decelPerSec * Time.deltaTime;
        else
        {
            float angle = Vector3.Angle(_moveInput.normalized, _lastMoveInput.normalized);

            _curAccel = angle > _resetAccelAng ? _startAccel : _curAccel + _accelPerSec * Time.deltaTime;
            _lastMoveInput = _moveInput;
        }

        _curAccel = Mathf.Clamp(_curAccel, 0, 1);

        Vector3 target = new();

        if (_sprintInput && Handler.StamController.TryConsume(_stamCostPerSec * Time.deltaTime))
        {
            target.x = _moveInput.x > 0 ? target.x = _sClampX[1] : target.x = _sClampX[0];
            target.z = _moveInput.z > 0 ? target.z = _sClampZ[1] : target.z = _sClampZ[0];
        }
        else
        {
            target.x = _moveInput.x > 0 ? target.x = _wClampX[1] : target.x = _wClampX[0];
            target.z = _moveInput.z > 0 ? target.z = _wClampZ[1] : target.z = _wClampZ[0];
        }

        target = Handler.transform.TransformDirection(Vector3.Scale(target, _lastMoveInput) * _curAccel);
        Handler.Move(target * Handler.Stats.MoveSpeedMul);
    }

    protected override void ResetMotion()
    {
        base.ResetMotion();
        _moveInput = Vector2.zero;
        _lastMoveInput = new();
        _sprintInput = false;
    }
}