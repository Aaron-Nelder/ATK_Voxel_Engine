using UnityEngine;
using UnityEngine.InputSystem;

public class MotionGravity : BaseMotion
{
    public override bool IsEnabled { get; protected set; } = false;
    public override MotionType Type { get; protected set; } = MotionType.Gravity;
    public override EntityMotionHandler Handler { get; protected set; }

    [SerializeField] float _accel = 0.00025f;
    [SerializeField] float _termVel = 25.0f;

    [Header("Jump Settings")]
    [SerializeField] float _stamCost = 10;
    [SerializeField] float _jumpForce = 6;

    bool _jumpInput = false;
    float _curFallSpeed = 0;

    void OnJump(InputValue value) => _jumpInput = value.isPressed;

    public override void ProccessUpdate()
    {
        base.ProccessUpdate();

        if (Handler.IsGrounded())
        {
            if (Handler.Velocity.y <= 0)
            {
                _curFallSpeed = -Handler.Stats.Mass;
                if (_jumpInput && Handler.StamController.TryConsume(_stamCost))
                    _curFallSpeed = _jumpForce;              
            }
        }
        else
            _curFallSpeed -= _accel * Handler.Stats.Mass * Handler.Stats.Gravity;

        if (Handler.HitHead())
            _curFallSpeed = 0;

        Handler.Velocity = new(Handler.Velocity.x, _curFallSpeed, Handler.Velocity.z);

        if (Handler.Velocity.y < -_termVel)
            Handler.Velocity = new(Handler.Velocity.x, -_termVel, Handler.Velocity.z);

        //Debug.Log(Handler.Velocity);
    }
}