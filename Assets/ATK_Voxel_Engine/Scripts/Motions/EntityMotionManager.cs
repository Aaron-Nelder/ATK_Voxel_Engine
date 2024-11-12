using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace ATKVoxelEngine
{
    public class EntityMotionManager : MonoBehaviour
    {
        EntityMotionHandler _handler;

        //MotionReferences
        [SerializeField] MotionJump _jumpMotion;
        [SerializeField] MotionMove _moveMotion;
        [SerializeField] MotionLook _lookMotion;
        [SerializeField] MotionCrouch _crouchMotion;
        [SerializeField] MotionType[] _defaultMoveSet = new MotionType[] { MotionType.MOVE, MotionType.LOOK, MotionType.JUMP, MotionType.CROUCH };

        // sets the debugging mode and is being listened to from PlayerInput
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

            if (motion.IsEnabled) return false;

            motion.OnMotionEnabled(_handler);
            OnMotionEnabled?.Invoke(motion);

            return true;
        }

        public bool DisableMotion(MotionType motionType)
        {
            BaseMotion motion = GetMotion(motionType);

            if (!motion.IsEnabled) return false;

            motion.OnMotionDisabled();
            OnMotionDisabled?.Invoke(motion);

            return true;
        }

        public BaseMotion GetMotion(MotionType motionType)
        {
            switch (motionType)
            {
                case MotionType.JUMP:
                    return _jumpMotion;
                case MotionType.MOVE:
                    return _moveMotion;
                case MotionType.LOOK:
                    return _lookMotion;
                case MotionType.CROUCH:
                    return _crouchMotion;
                default:
                    Debug.LogError("Motion not found");
                    return null;
            }
        }

        #region Editor

        [ContextMenu("Find Motions")]
        public void FindMotions()
        {
            _jumpMotion = transform.GetComponentInChildren<MotionJump>();
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
}