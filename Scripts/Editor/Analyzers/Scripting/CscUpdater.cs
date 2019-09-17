using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Analyzers.Scripting
{
    public class CscUpdater : IDisposable
    {
        private readonly ScriptingAnalyzer _analyzer;
        private static readonly string AnalyzersPrefix = ANALYZER_PREFIX + ScriptingAnalyzer.ANALYZERS_DIR + Path.DirectorySeparatorChar.ToString();
        private const string CSC_FILENAME = "csc.rsp";
        private const string ANALYZER_PREFIX = "-analyzer:";

        public CscUpdater(ScriptingAnalyzer analyzer)
        {
            _analyzer = analyzer;
            _analyzer.AnalyzersChanged += AnalyzerOnAnalyzersChanged;
        }

        private void AnalyzerOnAnalyzersChanged(object sender, string[] analyzers)
        {
            var rspFiles = AssetDatabase
                .FindAssets($"t:{nameof(DefaultAsset)}", new[] {"Assets"})
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(x => Path.GetFileName(x) == CSC_FILENAME)
                .ToArray();

            // warn the user if analyzers are installed but the UNITY_CODE_ANALYSIS_PACKAGE_NAME is missing.
            if (analyzers.Length > 0 && _analyzer.UnityCodeAnalysisPackagePath == null)
            {
                Debug.LogWarning(
                    $"You have analyzers installed but the \"{ScriptingAnalyzer.UNITY_CODE_ANALYSIS_PACKAGE_NAME}\" is missing.");
            }

            foreach (var rspFile in rspFiles)
            {
                UpdateCscFile(analyzers, rspFile);
            }
        }

        private void UpdateCscFile(string[] analyzers, string rspFile)
        {
            var text = File.ReadAllText(rspFile);
            var lineBreakCharacter = text.Contains("\r\n") ? "\r\n" : "\n"; // preserve line break
            var lines = text.Split(new[] {lineBreakCharacter}, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (_analyzer.UnityCodeAnalysisPackagePath != null)
            {
                AddAnalyzerMetaPackage(lines);
                AddAndRemoveAnalyzers(analyzers, lines);
            }
            else
            {
                RemoveAnalyzerMetaPackage(text, lines);
                RemoveAnalyzers(lines);
            }

            var newText = string.Join(lineBreakCharacter, lines) + lineBreakCharacter;

            if (newText != text)
            {
                // only write if changes (prevents re-triggering import)
                File.WriteAllLines(rspFile, lines);
            }
        }

        private void AddAnalyzerMetaPackage(List<string> lines)
        {
            // add reference to Microsoft.CodeAnalysis.dll from package manager
            var cscCodeAnalysisLine = $"-r:{_analyzer.UnityCodeAnalysisPackagePath}";
            if (!lines.Contains(cscCodeAnalysisLine))
            {
                lines.Add(cscCodeAnalysisLine);
            }
        }

        private void RemoveAnalyzerMetaPackage(string text, List<string> lines)
        {
            // remove meta reference if package not installed
            var metaLine = $"-r:{Path.Combine("Library", "PackageCache", ScriptingAnalyzer.UNITY_CODE_ANALYSIS_PACKAGE_NAME)}";
            if (text.Contains(metaLine))
            {
                lines.RemoveAll(x => x.StartsWith(metaLine));
            }
        }

        private void AddAndRemoveAnalyzers(string[] analyzers, List<string> lines)
        {
            // add analyzers from ANALYZERS_DIR to csc.rsp or remove if changed from last time
            var analyzerLines = analyzers.Select(x => ANALYZER_PREFIX + x).ToArray();
            var toRemove = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith(ANALYZER_PREFIX) && !analyzerLines.Contains(line))
                {
                    // cant modify lines in loop
                    toRemove.Add(line);
                }
            }

            foreach (var line in toRemove)
            {
                lines.Remove(line);
            }

            foreach (var analyzerLine in analyzerLines)
            {
                if (!lines.Contains(analyzerLine))
                {
                    lines.Add(analyzerLine);
                }
            }
        }

        private void RemoveAnalyzers(List<string> lines)
        {
            // if package not installed remove all analyzers from csc.rsp
            var toRemove = lines.Where(x => x.StartsWith(AnalyzersPrefix)).ToArray();
            foreach (var line in toRemove)
            {
                lines.Remove(line);
            }
        }

        public void Dispose()
        {
            _analyzer.AnalyzersChanged -= AnalyzerOnAnalyzersChanged;
        }
    }
}
