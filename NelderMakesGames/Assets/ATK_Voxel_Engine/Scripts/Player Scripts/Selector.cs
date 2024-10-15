using System;
using UnityEngine;

public class Selector : IUpdate
{
    public UpdateType UpdateType => UpdateType.FixedUpdate;

    Vector3 _screenCenter = new(Screen.width / 2, Screen.height / 2, 0);
    float _tickTimer = 0;
    float _tickRate = 0.1f;
    Camera _cam;

    public static SelectedVoxel SelectedVoxel { get; private set; }
    public static bool IsSelecting { get; private set; }
    public static event Action<SelectedVoxel> OnSelect;
    public static event Action<SelectedVoxel> OnDeselect;

    public Selector()
    {
        _cam = PlayerManager.Instance.PlayerCamera.Camera;
        // Registering the selector to the update manager
        UpdateManager.Register(this);
    }

    public void Update(float deltaTime)
    {
        _tickTimer += deltaTime;
        if (_tickTimer < _tickRate) return;
        _tickTimer = 0;

        // Checks if the ray hits a voxel
        bool selecting = WorldHelper.VoxelCast(_cam.ScreenPointToRay(_screenCenter), out SelectedVoxel newVoxel);

        if (selecting != IsSelecting)
        {
            IsSelecting = selecting;

            if (!IsSelecting)
            {
                OnDeselect?.Invoke(SelectedVoxel);
                SelectedVoxel = newVoxel;
                return;
            }
        }

        if (newVoxel.Id == 0) return;

        if (!SelectedVoxel.Equals(newVoxel))
        {
            OnSelect?.Invoke(SelectedVoxel = newVoxel);
        }
    }

    ~Selector()
    {
        UpdateManager.UnRegister(this);
    }
}

