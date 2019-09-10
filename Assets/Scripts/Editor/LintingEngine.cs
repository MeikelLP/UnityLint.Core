using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Editor
{
    [InitializeOnLoad]
    public static class LintingEngine
    {
        public static IAnalyzer[] Analyzers { get; }
        private static Queue<Action> _taskQueue;

        static LintingEngine()
        {
            var types = TypeCache.GetTypesDerivedFrom<IAnalyzer>().OrderBy(x => x.Name);
            Analyzers = types.Select(Activator.CreateInstance).Cast<IAnalyzer>().ToArray();
            _taskQueue = new Queue<Action>();

            EditorApplication.update += Update;
        }

        private static void Update()
        {
            while (_taskQueue.Count > 0)
            {
                _taskQueue.Dequeue().Invoke();
            }
        }

        public static T GetAnalyzer<T>() where T : IAnalyzer
        {
            return (T) Analyzers.FirstOrDefault(x => x is T);
        }

        public static void EnqueueOnUnityThread(Action action)
        {
            _taskQueue.Enqueue(action);
        }
    }
}
