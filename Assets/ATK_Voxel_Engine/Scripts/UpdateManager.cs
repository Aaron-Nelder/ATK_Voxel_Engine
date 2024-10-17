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
        for (int i = 0; i < updates.Count; i++)
            updates[i].Update(Time.deltaTime);
    }

    void FixedUpdate()
    {
        for(int i = 0; i < fixedUpdates.Count; i++)
            fixedUpdates[i].Update(Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        for (int i = 0; i < lateUpdates.Count; i++)
            lateUpdates[i].Update(Time.fixedDeltaTime);
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
