using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Editor.Analyzers.Asset.Extensions;
using Editor.Issue;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset
{
    public class AssetAnalyzer : IAnalyzer
    {
        private const string ROW_UXML_GUID = "260ea58151f34b09b9f2c51c198d96e1";

        private readonly IAssetRule[] _rules;
        private readonly List<IAssetIssue<Object>> _issues;
        private readonly VisualTreeAsset _rowTemplate;

        public int IssueCount => _issues.Count;

        public VisualElement RootElement { get; }

        public AssetAnalyzer(IServiceProvider serviceProvider)
        {
            _issues = new List<IAssetIssue<Object>>();

            AllAssetImporter.AssetPathsChanged += AllAssetImporterOnAssetPathsChanged;
            var assetRules = TypeCache.GetTypesDerivedFrom<IAssetRule>();
            _rules = assetRules
                .Where(x => !x.IsAbstract)
                .Select(x => ActivatorUtilities.CreateInstance(serviceProvider, x))
                .Cast<IAssetRule>()
                .ToArray();

            var rowTemplatePath = AssetDatabase.GUIDToAssetPath(ROW_UXML_GUID);
            _rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(rowTemplatePath);

            RootElement = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "asset-issues"
            };
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
            UpdateUI();
        }

        public void Update()
        {
            var assetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(AllAssetImporter.IsProjectAssetAndNotAFolder)
                .OrderBy(x => x)
                .ToArray();
            AnalyzeAssets(assetPaths);
        }

        private void UpdateUI()
        {
            if (RootElement != null)
            {
                RootElement.Clear();

                var groups = _issues.GroupBy(x => x.Type).OrderBy(x => (int) x.Key).ToArray();

                foreach (var group in groups)
                {
                    var items = @group.OrderBy(x => x.AssetType.Name).ThenBy(x => x.AssetPath).ToArray();
                    var container = new VisualElement();

                    var header = IssueUIUtility.GetHeader(group.Key, items.Length, container);
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
