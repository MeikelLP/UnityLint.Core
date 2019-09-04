using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;

namespace Editor
{
    [InitializeOnLoad]
    public static class LintingEngine
    {
        public static IAnalyzer[] Analyzers { get; }

        static LintingEngine()
        {
            var types = TypeCache.GetTypesDerivedFrom<IAnalyzer>().OrderBy(x => x.Name);
            Analyzers = types.Select(Activator.CreateInstance).Cast<IAnalyzer>().ToArray();
        }
    }
}
