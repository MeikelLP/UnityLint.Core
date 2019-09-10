using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Editor.Extensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.Analyzers.Project
{
    public class ProjectAnalyzer : IAnalyzer
    {
        private const string ROW_UXML_GUID = "260ea58151f34b09b9f2c51c198d96e1";

        private readonly ReadOnlyCollection<IProjectRule> _rules;
        private readonly IList<ProjectIssue> _errors;
        private readonly VisualTreeAsset _uxml;

        public int IssueCount => _errors.Count;

        public VisualElement RootElement { get; }

        public ProjectAnalyzer()
        {
            _errors = new List<ProjectIssue>();

            AllAssetImporter.AssetPathsChanged += AllAssetImporterOnAssetPathsChanged;
            var instances = TypeCache.GetTypesDerivedFrom<IProjectRule>()
                .Select(Activator.CreateInstance)
                .Cast<IProjectRule>()
                .ToArray();
            _rules = Array.AsReadOnly(instances);

            var uxmlPath = AssetDatabase.GUIDToAssetPath(ROW_UXML_GUID);
            _uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            var assetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(AllAssetImporter.IsProjectAssetAndNotAFolder)
                .ToArray();
            AnalyzeProject(assetPaths);

            RootElement = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "project-issues"
            };
            var stylesheetPath = AssetDatabase.GUIDToAssetPath("ce5902e5af4941fcacb06882d1d2e24e");
            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesheetPath);
            RootElement.styleSheets.Add(stylesheet);
        }

        private void AllAssetImporterOnAssetPathsChanged(object sender, string[] assetPaths)
        {
            AnalyzeProject(assetPaths);
        }

        private void AnalyzeProject(string[] assetPaths)
        {
            _errors.Clear();
            foreach (var path in assetPaths)
            {
                foreach (var rule in _rules)
                {
                    var error = rule.Validate(path);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        var issue = new ProjectIssue
                        {
                            Message = error,
                            Type = AssetDatabase.GetMainAssetTypeAtPath(path),
                            AssetPath = path
                        };
                        _errors.Add(issue);
                    }
                }
            }
        }

        public void Update()
        {
            if (RootElement != null)
            {
                RootElement.Clear();
                foreach (var error in _errors)
                {
                    var row = _uxml.CloneTree()[0];

                    ((Image) row[0]).image = error.Type.ToIcon();
                    ((Image) row[0]).tooltip = error.Type.ToString();

                    ((Button) row[1]).text = error.AssetPath;

                    ((Label) row[2]).text = error.Message;

                    RootElement.Add(row);
                }
            }
        }

        public void Dispose()
        {
            AllAssetImporter.AssetPathsChanged -= AllAssetImporterOnAssetPathsChanged;
        }
    }
}
