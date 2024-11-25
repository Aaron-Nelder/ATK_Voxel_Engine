using System.Threading.Tasks;
using UnityEngine;
using System.Threading;

namespace ATKVoxelEngine
{
    public static class TaskUtility
    {
        public static async Task AwaitTask(Task t, CancellationToken token)
        {
            while (!t.IsCompletedSuccessfully)
            {
                // If the task is cancelled, return
                if (token.IsCancellationRequested) return;

                // If the task has an exception, log it
                if (t.Exception != null)
                {
                    Debug.LogError(t.Exception);
                    return;
                }

                // If the task is faulted, log it
                if (t.IsFaulted)
                {
                    Debug.LogError($"Task '{t.Id}' Faulted");
                    return;
                }

                await t;
            }
        }
    }
}
