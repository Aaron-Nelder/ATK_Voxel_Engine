using System;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    static List<IUpdate> updates = new List<IUpdate>();
    static List<IUpdate> fixedUpdates = new List<IUpdate>();
    static List<IUpdate> lateUpdates = new List<IUpdate>();

    public static bool Register(IUpdate obj)
    {
        switch (obj.UpdateType)
        {
            case UpdateType.Update:
                if (updates.Contains(obj))
                    return false;
                updates.Add(obj);
                return true;
            case UpdateType.FixedUpdate:
                if (fixedUpdates.Contains(obj))
                    return false;
                fixedUpdates.Add(obj);
                return true;
            case UpdateType.LateUpdate:
                if (lateUpdates.Contains(obj))
                    return false;
                lateUpdates.Add(obj);
                return true;
            default:
                return false;
        }
    }

    public static bool UnRegister(IUpdate obj)
    {
        switch (obj.UpdateType)
        {
            case UpdateType.Update:
                if (!updates.Contains(obj))
                    return false;
                updates.Remove(obj);
                return true;
            case UpdateType.FixedUpdate:
                if (!fixedUpdates.Contains(obj))
                    return false;
                fixedUpdates.Remove(obj);
                return true;
            case UpdateType.LateUpdate:
                if (!lateUpdates.Contains(obj))
                    return false;
                lateUpdates.Remove(obj);
                return true;
            default:
                return false;
        }
    }

    void Update()
    {
        foreach (var updatable in updates)
            updatable.Update(Time.deltaTime);
    }

    void FixedUpdate()
    {
        foreach (var updatable in fixedUpdates)
            updatable.Update(Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        foreach (var updatable in lateUpdates)
            updatable.Update(Time.deltaTime);
    }
}

public interface IUpdate
{
    public UpdateType UpdateType { get; }
    public void Update(float deltaTime);
}

public enum UpdateType
{
    Update,
    FixedUpdate,
    LateUpdate
}
