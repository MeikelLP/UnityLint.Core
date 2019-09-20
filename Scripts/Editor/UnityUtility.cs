using System;
using System.Collections.Concurrent;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public static class UnityUtility
    {
        static UnityUtility()
        {
            TaskQueue = new ConcurrentQueue<Action>();

            EditorApplication.update += Update;
        }

        private static readonly ConcurrentQueue<Action> TaskQueue;

        public static void EnqueueOnUnityThread(Action action)
        {
            TaskQueue.Enqueue(action);
        }

        private static void Update()
        {
            while (!TaskQueue.IsEmpty)
            {
                while (TaskQueue.TryDequeue(out var action))
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
