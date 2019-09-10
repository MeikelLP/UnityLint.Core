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
        private const string HEADER_UXML_GUID = "2f7c085e117c42ac99abfa2f7613b201";

        private readonly ReadOnlyCollection<IProjectRule> _rules;
        private readonly List<ProjectIssue> _issues;
        private readonly VisualTreeAsset _rowTemplate;
        private readonly VisualTreeAsset _headerTemplate;

        public int IssueCount => _issues.Count;

        public VisualElement RootElement { get; }

        public ProjectAnalyzer()
        {
            _issues = new List<ProjectIssue>();

            AllAssetImporter.AssetPathsChanged += AllAssetImporterOnAssetPathsChanged;
            var instances = TypeCache.GetTypesDerivedFrom<IProjectRule>()
                .Select(Activator.CreateInstance)
                .Cast<IProjectRule>()
                .ToArray();
            _rules = Array.AsReadOnly(instances);

            var rowTemplatePath = AssetDatabase.GUIDToAssetPath(ROW_UXML_GUID);
            _rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(rowTemplatePath);

            var headerTemplatePath = AssetDatabase.GUIDToAssetPath(HEADER_UXML_GUID);
            _headerTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(headerTemplatePath);

            var assetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(AllAssetImporter.IsProjectAssetAndNotAFolder)
                .OrderBy(x => x)
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
            _issues.Clear();
            foreach (var path in assetPaths)
            {
                foreach (var rule in _rules)
                {
                    var issue = new ProjectIssue(path);
                    if (!rule.IsValid(issue))
                    {
                        _issues.Add(issue);
                    }
                }
            }
        }

        public void Update()
        {
            if (RootElement != null)
            {
                RootElement.Clear();

                var groups = _issues.GroupBy(x => x.Type).OrderBy(x => (int) x.Key).ToArray();

                foreach (var group in groups)
                {
                    var items = group.ToArray();
                    var container = new VisualElement();

                    var header = _headerTemplate.CloneTree();
                    var image = header.Q<Image>("icon");
                    var heading = header.Q<Button>("heading");

                    image.image = group.Key.ToIcon();
                    heading.text = $"{group.Key.ToString()} ({items.Length.ToString()})";
                    heading.clickable = new Clickable(() =>
                    {
                        if (container.GetClasses().Contains("is-hidden"))
                        {
                            container.RemoveFromClassList("is-hidden");
                        }
                        else
                        {
                            container.AddToClassList("is-hidden");
                        }
                    });

                    RootElement.Add(header);
                    RootElement.Add(container);

                    foreach (var issue in items)
                    {
                        var row = _rowTemplate.CloneTree()[0];

                        var assetTypeImage = row.Q<Image>("asset-type");
                        assetTypeImage.image = issue.AssetType.ToIcon();
                        assetTypeImage.tooltip = issue.AssetType.ToString();

                        var button = row.Q<Button>("button");
                        button.text = issue.AssetPath.Replace("Assets/", "");
                        button.tooltip = issue.AssetPath;
                        button.clickable = new Clickable(() => Selection.activeObject = issue.Asset);

                        var message = row.Q<Label>("message");
                        message.text = issue.Message;

                        var fix = row.Q<Button>("fix-button");
                        if (issue.Fix != null)
                        {
                            fix.clickable = new Clickable(() => issue.Fix(issue));
                        }
                        else
                        {
                            fix.visible = false;
                        }

                        container.Add(row);
                    }
                }
            }
        }

        public void Dispose()
        {
            AllAssetImporter.AssetPathsChanged -= AllAssetImporterOnAssetPathsChanged;
        }
    }
}
