using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public static class LintingEngine
    {
        public static IAnalyzer[] Analyzers { private set; get; }
        private static readonly ConcurrentQueue<Action> TaskQueue;
        private static readonly ServiceProvider Provider;


        static LintingEngine()
        {
            TaskQueue = new ConcurrentQueue<Action>();

            var services = new ServiceCollection();
            var serviceConfigurationTypes = TypeCache.GetTypesDerivedFrom<IServiceConfiguration>().ToArray();
            var tempProvider = services.BuildServiceProvider();
            foreach (var type in serviceConfigurationTypes)
            {
                try
                {
                    var configurator = (IServiceConfiguration) ActivatorUtilities.CreateInstance(tempProvider, type);
                    configurator.ConfigureServices(services);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Provider = services.BuildServiceProvider();
            var analyzerTypes = TypeCache.GetTypesDerivedFrom<IAnalyzer>().OrderBy(x => x.Name);
            Analyzers = analyzerTypes
                .Select(t => ActivatorUtilities.CreateInstance(Provider, t))
                .Cast<IAnalyzer>()
                .ToArray();

            EditorApplication.update += Update;

            foreach (var analyzer in Analyzers)
            {
                TaskQueue.Enqueue(() => analyzer.Initialize());
            }
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

        public static T GetAnalyzer<T>() where T : IAnalyzer
        {
            return (T) Analyzers.FirstOrDefault(x => x is T);
        }

        public static void EnqueueOnUnityThread(Action action)
        {
            TaskQueue.Enqueue(action);
        }
    }

    public interface IServiceConfiguration
    {
        void ConfigureServices(ServiceCollection services);
    }
}
