using UnityEngine;

public abstract class BaseMotion : MonoBehaviour
{
    public abstract bool IsEnabled { get; protected set; }
    public abstract MotionType Type { get; protected set; }
    public abstract EntityMotionHandler Handler { get; protected set; }

    public virtual void ProccessUpdate() { }

    public virtual void ProccessFixedUpdate() { }

    public virtual void ProccessLateUpdate() { }

    protected virtual void ResetMotion() { }

    public virtual void OnMotionEnabled(EntityMotionHandler handler) 
    { 
        Handler = handler;
        IsEnabled = true; 
        ResetMotion();
    }

    public virtual void OnMotionDisabled() 
    { 
        IsEnabled = false;
    }
}

public enum MotionType
{
    Moving,
    Crouching,
    Looking,
    Gravity,
}
