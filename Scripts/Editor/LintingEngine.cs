using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public static class LintingEngine
    {
        public static IAnalyzer[] Analyzers { get; }
        public static bool Initialized { get; private set; }

        private static readonly LintingEngineSettings Settings;

        public static event EventHandler<IAnalyzer> AnalyzerUpdated;

        static LintingEngine()
        {
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

            var provider = services.BuildServiceProvider();
            Settings = provider.GetService<LintingEngineSettings>();

            var analyzerTypes = TypeCache.GetTypesDerivedFrom<IAnalyzer>()
                .OrderBy(x => x.Name)
                .ToArray();
            var analyzers = new List<IAnalyzer>();
            foreach (var type in analyzerTypes)
            {
                try
                {
                    var analyzer = (IAnalyzer) ActivatorUtilities.CreateInstance(provider, type);
                    analyzers.Add(analyzer);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to instantiate analyzer of type: {type}");
                    Debug.LogException(e);
                }
            }

            Analyzers = analyzers.ToArray();

            foreach (var analyzer in Analyzers)
            {
                // weird bug in 2019.3 causes asset paths to be null similar to issue #1060352 but looks like a
                // regression
                UnityUtility.EnqueueOnUnityThread(() =>
                {
                    analyzer.Initialize();
                    UpdateAnalyzer(analyzer);
                });
            }
            UnityUtility.EnqueueOnUnityThread(() => Initialized = true);
        }

        public static void UpdateAnalyzer(IAnalyzer analyzer)
        {
            if (analyzer == null) return;

            if (analyzer is IEnumerable enumerable)
            {
                var allowedTime = TimeSpan.FromSeconds(1d / Settings.TargetFrameRate);
                var enumerator = enumerable.GetEnumerator();
                Enumerate(allowedTime, enumerator, analyzer);
            }
            else
            {
                analyzer.Update();
                AnalyzerUpdated?.Invoke(null, analyzer);
            }
        }

        private static void Enumerate(TimeSpan timeLimit, IEnumerator enumerator, IAnalyzer analyzer, bool startNextFrame = false)
        {
            if (startNextFrame)
            {
                UnityUtility.EnqueueOnUnityThread(() => Enumerate(timeLimit, enumerator, analyzer));
            }
            else
            {
                var startTime = DateTime.UtcNow;
                while (enumerator.MoveNext())
                {
                    if (DateTime.UtcNow - startTime >= timeLimit)
                    {
                        AnalyzerUpdated?.Invoke(null, analyzer);
                        UnityUtility.EnqueueOnUnityThread(() => Enumerate(timeLimit, enumerator, analyzer));
                        return;
                    }
                }
                AnalyzerUpdated?.Invoke(null, analyzer);
            }
        }

        public static IAnalyzer GetAnalyzer(Type type)
        {
            return Analyzers.SingleOrDefault(x => typeof(IAnalyzer).IsAssignableFrom(type));
        }

        public static T GetAnalyzer<T>() where T : IAnalyzer
        {
            return (T) Analyzers.FirstOrDefault(x => x is T);
        }
    }
}
