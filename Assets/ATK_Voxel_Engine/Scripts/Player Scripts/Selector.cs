using System;
using UnityEngine;

namespace ATKVoxelEngine
{
    public static class Selector
    {
        static Vector2 _screenCenter = new(Screen.width / 2, Screen.height / 2);
        static Camera _cam;

        public static VoxelCastHit SelectedVoxel { get; private set; }
        public static bool HasSelection { get; private set; }
        public static event Action<VoxelCastHit> OnSelect;
        public static event Action<VoxelCastHit> OnDeselect;

        public static void Register()
        {
            _cam = PlayerManager.Instance.PlayerCamera;
            TickRateManager.OnFixedUpdate += Update;
        }

        public static void UnRegister()
        {
            TickRateManager.OnFixedUpdate -= Update;
        }

        public static void Update(float deltaTime)
        {
            _screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

            // Checks if the ray hits a voxel
            bool hitVoxel = EngineUtilities.VoxelCast(_cam.ScreenPointToRay(_screenCenter), out VoxelCastHit voxel);

            if (hitVoxel != HasSelection)
            {
                HasSelection = hitVoxel;

                if (!HasSelection)
                {
                    OnDeselect?.Invoke(SelectedVoxel);
                    SelectedVoxel = voxel;
                    return;
                }
                else
                    CheckForSelection(voxel);
            }

            if (voxel.Id == 0) return;

            CheckForSelection(voxel);
        }

        static void CheckForSelection(VoxelCastHit selectedVoxel)
        {
            if (!SelectedVoxel.Equals(selectedVoxel))
                OnSelect?.Invoke(SelectedVoxel = selectedVoxel);
        }
    }
}