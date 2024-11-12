using UnityEngine;
using UnityEngine.InputSystem;

namespace ATKVoxelEngine
{
    public class MotionCrouch : BaseMotion
    {
        public override bool IsEnabled { get; protected set; } = false;
        public override MotionType Type => MotionType.CROUCH;
        public override TickType TickType => TickType.FIXED_UPDATE;
        public override EntityMotionHandler Handler { get; protected set; }

        [SerializeField] float _cHeight = 0.5f;
        [SerializeField] float _speed = 0.5f;

        bool _isCrouching = false;

        void OnCrouch(InputValue value) => _isCrouching = value.isPressed;

        public override void Tick(float deltaTime)
        {
            if (!_isCrouching && Handler.Collider.size.y == Handler.EntityStats.MotionStats.Size.y)
                return;

            Handler.Rigidbody.WakeUp();

            if (_isCrouching && Handler.Collider.bounds.size.y != _cHeight)
            {
                Vector3 target = new(Handler.Collider.size.x, _cHeight, Handler.Collider.size.z);
                Handler.Collider.size = Vector3.Lerp(Handler.Collider.size, target, _speed * deltaTime);
                if (Vector3.Distance(Handler.Collider.size, target) <= 0.05)
                    Handler.Collider.size = target;
            }
            else if (!_isCrouching && Handler.Collider.size.y != Handler.EntityStats.MotionStats.Size.y)
            {
                if (!Handler.HitHead)
                {
                    Vector3 target = new(Handler.Collider.size.x, Handler.EntityStats.MotionStats.Size.y, Handler.Collider.size.z);
                    Handler.Collider.size = Vector3.Lerp(Handler.Collider.size, target, _speed * deltaTime);
                    if (Vector3.Distance(Handler.Collider.size, target) <= 0.05)
                        Handler.Collider.size = target;
                }
            }
        }
    }
}