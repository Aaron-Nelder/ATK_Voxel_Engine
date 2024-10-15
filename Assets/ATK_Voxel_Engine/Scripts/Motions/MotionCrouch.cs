using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MotionCrouch : BaseMotion
{
    public override bool IsEnabled { get; protected set; } = false;
    public override MotionType Type { get; protected set; } = MotionType.Crouching;
    public override EntityMotionHandler Handler { get; protected set; }

    [SerializeField] float _cHeight = 0.5f;
    [SerializeField] float _speed = 0.5f;
    [SerializeField] float _standMove = 5f;

    bool _isCrouching = false;

    void OnCrouch(InputValue value) => _isCrouching = value.isPressed;

    public override void ProccessUpdate()
    {
        if (_isCrouching && Handler.Controller.height != _cHeight)
        {
            Handler.Controller.height = Mathf.Lerp(Handler.Controller.height, _cHeight, _speed * Time.deltaTime);
        }
        else if (!_isCrouching && Handler.Controller.height != Handler.EntityStats.Height)
        {
            if (CanStand())
            {
                float newHeight = Mathf.Lerp(Handler.Controller.height, Handler.EntityStats.Height, _speed * Time.deltaTime);
                Handler.Move(Vector3.up * (newHeight - Handler.Controller.height) * _standMove);
                Handler.Controller.height = newHeight;
            }
        }
    }

    bool CanStand()
    {
        Vector3 origin = Handler.transform.position + Vector3.up * Handler.EntityStats.Height;
        Vector3 direction = Vector3.up * (_cHeight - Handler.EntityStats.Height);

        return !Physics.Raycast(origin, direction, direction.magnitude, Handler.Stats.GroundedLayers, QueryTriggerInteraction.Ignore);
    }
}
