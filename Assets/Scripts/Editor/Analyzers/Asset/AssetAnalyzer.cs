using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Editor.Analyzers.Asset.Extensions;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset
{
    public class AssetAnalyzer : IAnalyzer
    {
        private const string ROW_UXML_GUID = "260ea58151f34b09b9f2c51c198d96e1";
        private const string HEADER_UXML_GUID = "2f7c085e117c42ac99abfa2f7613b201";
        private const string STYLE_SHEET_GUID = "ce5902e5af4941fcacb06882d1d2e24e";

        private readonly ReadOnlyCollection<IAssetRule> _rules;
        private readonly List<IAssetIssue<Object>> _issues;
        private readonly VisualTreeAsset _rowTemplate;
        private readonly VisualTreeAsset _headerTemplate;

        public int IssueCount => _issues.Count;

        public VisualElement RootElement { get; }

        public AssetAnalyzer(IServiceProvider serviceProvider)
        {
            _issues = new List<IAssetIssue<Object>>();

            AllAssetImporter.AssetPathsChanged += AllAssetImporterOnAssetPathsChanged;
            var instances = TypeCache.GetTypesDerivedFrom<IAssetRule>()
                .Where(x => !x.IsAbstract)
                .Select(x => ActivatorUtilities.CreateInstance(serviceProvider, x))
                .Cast<IAssetRule>()
                .ToArray();
            _rules = Array.AsReadOnly(instances);

            var rowTemplatePath = AssetDatabase.GUIDToAssetPath(ROW_UXML_GUID);
            _rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(rowTemplatePath);

            var headerTemplatePath = AssetDatabase.GUIDToAssetPath(HEADER_UXML_GUID);
            _headerTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(headerTemplatePath);

            RootElement = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "project-issues"
            };
            var stylesheetPath = AssetDatabase.GUIDToAssetPath(STYLE_SHEET_GUID);
            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesheetPath);
            RootElement.styleSheets.Add(stylesheet);
        }

        private void AllAssetImporterOnAssetPathsChanged(object sender, string[] assetPaths)
        {
            AnalyzeAssets(assetPaths);
        }

        private void AnalyzeAssets(string[] assetPaths)
        {
            _issues.Clear();
            foreach (var rule in _rules)
            {
                var baseType = rule.GetType().BaseType;
                // ReSharper disable once PossibleNullReferenceException
                var typeFilter = baseType.IsGenericType ?
                    baseType.GetGenericArguments()[0] :
                    typeof(Object);

                foreach (var path in assetPaths)
                {
                    // filter by asset type
                    if (!typeFilter.IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(path))) continue;

                    if (rule.HasIssue(path, out var issue))
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
                var assetPaths = AssetDatabase.GetAllAssetPaths()
                    .Where(AllAssetImporter.IsProjectAssetAndNotAFolder)
                    .OrderBy(x => x)
                    .ToArray();
                AnalyzeAssets(assetPaths);

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
                        message.tooltip = issue.Message;

                        var fixButton = row.Q<Button>("fix-button");
                        if (issue.FixAction != null)
                        {
                            fixButton.clickable = new Clickable(() =>
                            {
                                if (issue.FixAction())
                                {
                                    row.RemoveFromHierarchy();
                                }
                            });
                        }
                        else
                        {
                            fixButton.visible = false;
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
