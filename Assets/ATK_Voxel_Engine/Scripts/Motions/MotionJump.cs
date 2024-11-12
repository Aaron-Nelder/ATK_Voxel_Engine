using ATKVoxelEngine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MotionJump : BaseMotion
{
    public override bool IsEnabled { get; protected set; } = false;
    public override MotionType Type => MotionType.JUMP;
    public override TickType TickType => TickType.FIXED_UPDATE;
    public override EntityMotionHandler Handler { get; protected set; }

    [SerializeField] float _stamCost = 10;
    [SerializeField] float _jumpForce = 6;

    bool _jumpInput = false;

    void OnJump(InputValue value) => _jumpInput = value.isPressed;

    public override void Tick(float deltaTime)
    {
        if (!_jumpInput) return;

        if (Handler.IsGrounded && Handler.Rigidbody.linearVelocity.y <= 0 && Handler.StamController.TryConsume(_stamCost))
        {
            // set all the values to 0 to avoid any weird behavior
            Handler.Rigidbody.angularDamping = 0;
            Handler.Rigidbody.linearVelocity = Vector3.zero;
            Handler.Rigidbody.AddForce(_jumpForce * Vector3.up, ForceMode.Impulse);
        }
    }
}