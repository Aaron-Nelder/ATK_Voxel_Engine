using UnityEngine;

namespace ATKVoxelEngine
{
    public enum MotionType { MOVE, CROUCH, LOOK, JUMP, }
    
    public abstract class BaseMotion : MonoBehaviour, ITickable
    {
        public abstract bool IsEnabled { get; protected set; }
        public abstract MotionType Type { get; }
        public abstract TickType TickType { get; }
        public abstract EntityMotionHandler Handler { get; protected set; }

        public abstract void Tick(float deltaTime);

        protected virtual void ResetMotion() { }

        public virtual void OnMotionEnabled(EntityMotionHandler handler)
        {
            Handler = handler;
            IsEnabled = true;
            ResetMotion();
            Register();
        }

        public virtual void OnMotionDisabled()
        {
            IsEnabled = false;
            UnRegister();
        }

        public void Register()
        {
            switch (TickType)
            {
                case TickType.UPDATE:
                    TickRateManager.OnUpdate += Tick;
                    break;
                case TickType.FIXED_UPDATE:
                    TickRateManager.OnFixedUpdate += Tick;
                    break;
                case TickType.LATE_UPDATE:
                    TickRateManager.OnLateUpdate += Tick;
                    break;
            }
        }

        public void UnRegister()
        {
            switch (TickType)
            {
                case TickType.UPDATE:
                    TickRateManager.OnUpdate -= Tick;
                    break;
                case TickType.FIXED_UPDATE:
                    TickRateManager.OnFixedUpdate -= Tick;
                    break;
                case TickType.LATE_UPDATE:
                    TickRateManager.OnLateUpdate -= Tick;
                    break;
            }
        }
    }
}