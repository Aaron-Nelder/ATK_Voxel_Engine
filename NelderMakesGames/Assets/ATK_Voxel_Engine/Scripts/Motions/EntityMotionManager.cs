using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class EntityMotionManager : MonoBehaviour
{
    EntityMotionHandler _handler;

    //  MotionReferences
    [SerializeField] MotionGravity _gravityMotion;
    [SerializeField] MotionMove _moveMotion;
    [SerializeField] MotionLook _lookMotion;
    [SerializeField] MotionCrouch _crouchMotion;
    [SerializeField] MotionType[] _defaultMoveSet = new MotionType[] { MotionType.Moving, MotionType.Looking, MotionType.Gravity, MotionType.Crouching };

    //dictionary that holds the current enabled motions
    ConcurrentDictionary<MotionType, BaseMotion> EnabledMotions = new ConcurrentDictionary<MotionType, BaseMotion>();

    void OnDebugMode() => DebugHelper.Debugging = !DebugHelper.Debugging;

    public Action<BaseMotion> OnMotionEnabled;
    public Action<BaseMotion> OnMotionDisabled;

    public void Init(EntityMotionHandler handler, bool enableDefaultMoveset)
    {
        _handler = handler;
        if (enableDefaultMoveset)
            EnableDefaultMoveset();
    }

    void EnableDefaultMoveset()
    {
        foreach (var motion in _defaultMoveSet)
            EnableMotion(motion);
    }

    public bool EnableMotion(MotionType motionType)
    {
        BaseMotion motion = GetMotion(motionType);
        bool added = EnabledMotions.TryAdd(motionType, motion);

        if (added)
        {
            motion.OnMotionEnabled(_handler);
            OnMotionEnabled?.Invoke(motion);
        }

        return added;
    }

    public bool DisableMotion(MotionType motionType)
    {
        bool removed = EnabledMotions.TryRemove(motionType, out BaseMotion value);

        if (removed)
        {
            value.OnMotionDisabled();
            OnMotionDisabled?.Invoke(value);
        }
        return removed;
    }

    public BaseMotion GetMotion(MotionType motionType)
    {
        switch (motionType)
        {
            case MotionType.Gravity:
                return _gravityMotion;
            case MotionType.Moving:
                return _moveMotion;
            case MotionType.Looking:
                return _lookMotion;
            case MotionType.Crouching:
                return _crouchMotion;
            default:
                Debug.LogError("Motion not found");
                return null;
        }
    }

    void Update()
    {
        foreach (var motion in EnabledMotions)
            motion.Value.ProccessUpdate();
    }

    void FixedUpdate()
    {
        foreach (var motion in EnabledMotions)
            motion.Value.ProccessFixedUpdate();
    }

    void LateUpdate()
    {
        foreach (var motion in EnabledMotions)
            motion.Value.ProccessLateUpdate();
    }

    #region Editor

    [ContextMenu("Find Motions")]
    public void FindMotions()
    {
        _gravityMotion = transform.GetComponentInChildren<MotionGravity>();
        _moveMotion = transform.GetComponentInChildren<MotionMove>();
        _lookMotion = transform.GetComponentInChildren<MotionLook>();
        _crouchMotion = transform.GetComponentInChildren<MotionCrouch>();

        List<MotionType> mList = new List<MotionType>();

        foreach (var motion in _defaultMoveSet)
        {
            if (GetMotion(motion) is not null)
                mList.Add(motion);
        }

        _defaultMoveSet = mList.ToArray();
    }
    #endregion
}