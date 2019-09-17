using System;
using UnityEditor;
using UnityEngine;

namespace Editor.Analyzers.Scripting
{
    public class CsprojPostProcessor : AssetPostprocessor
    {
        private static string OnGeneratedCSProject(string path, string content)
        {
            try
            {
                // update .csproj files when they are generated
                // adds analyzers to csproj files for supporting IDEs
                var analyzers = LintingEngine.GetAnalyzer<ScriptingAnalyzer>().AnalyzerAssemblies;
                return UnityCsprojUpdater.UpdateCsProject(analyzers, content);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return content;
            }
        }
    }
}
