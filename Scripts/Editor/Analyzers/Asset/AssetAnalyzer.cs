using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Editor.Analyzers.Asset.Extensions;
using Editor.Issue;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset
{
    public class AssetAnalyzer : IAnalyzer, IEnumerable
    {
        private const string ROW_UXML_GUID = "260ea58151f34b09b9f2c51c198d96e1";

        private readonly IAssetRule[] _rules;
        private readonly ObservableCollection<IAssetIssue<Object>> _issues;
        private VisualTreeAsset _rowTemplate;

        public int IssueCount => _issues.Count;

        public VisualElement RootElement { get; private set; }

        public AssetAnalyzer(IServiceProvider serviceProvider)
        {
            _issues = new ObservableCollection<IAssetIssue<Object>>();
            _issues.CollectionChanged += IssuesOnCollectionChanged;

            AllAssetImporter.AssetPathsChanged += AllAssetImporterOnAssetPathsChanged;
            var assetRules = TypeCache.GetTypesDerivedFrom<IAssetRule>();
            _rules = assetRules
                .Where(x => !x.IsAbstract)
                .Select(x => ActivatorUtilities.CreateInstance(serviceProvider, x))
                .Cast<IAssetRule>()
                .ToArray();
        }

        private void IssuesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var list = (ObservableCollection<IAssetIssue<Object>>) sender;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    var issue = list[e.NewStartingIndex];
                    InsertIssueToUI(e.NewStartingIndex, issue, list.Count(x => x.Type == issue.Type));
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    var issue = list[e.NewStartingIndex];
                    DeleteIssueFromUI(e.NewStartingIndex, issue, list.Count(x => x.Type == issue.Type));
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                    ClearIssuesFromUI();
                    break;
            }
        }

        private void ClearIssuesFromUI()
        {
            RootElement.Clear();
        }

        private void DeleteIssueFromUI(int i, IAssetIssue<Object> issue, int length)
        {
            var container = RootElement.Q<VisualElement>($"issues-{issue.Type}");
            var header = container.Q<Button>("heading");
            if (container == null) return;

            var row = container.Q<VisualElement>($"issue-{i.ToString()}");
            row.RemoveFromHierarchy();

            var childCount = container
                .Query<VisualElement>($"issues-{issue.Type}")
                .Children<VisualElement>()
                .ToList()
                .Count;
            if (childCount == 0)
            {
                header.RemoveFromHierarchy();
                container.RemoveFromHierarchy();
            }
            else
            {
                header.text = $"{issue.Type.ToString()} ({length.ToString()})";
            }
        }

        private void InsertIssueToUI(int index, IAssetIssue<Object> issue, int length)
        {
            var container = RootElement.Q<VisualElement>($"issues-{issue.Type}");
            if (container == null)
            {
                container = new VisualElement
                {
                    name = $"issues-{issue.Type}"
                };
                var header = IssueUIUtility.GetHeader(issue.Type, length, container);
                RootElement.Add(header);
                RootElement.Add(container);
            }
            else
            {
                var header = RootElement.Query<VisualElement>($"header-{issue.Type}").Children<Button>("heading").First();
                header.text = $"{issue.Type.ToString()} ({length.ToString()})";
            }

            var row = _rowTemplate.CloneTree()[0];
            row.name = $"issue-{index.ToString()}";

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

        public void Initialize()
        {
            var rowTemplatePath = AssetDatabase.GUIDToAssetPath(ROW_UXML_GUID);
            _rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(rowTemplatePath);

            RootElement = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "asset-issues"
            };
        }

        public IEnumerator GetEnumerator()
        {
            var assetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(AllAssetImporter.IsProjectAssetAndNotAFolder)
                .OrderBy(x => x)
                .ToArray();
            _issues.Clear();

            foreach (var rule in _rules)
            {
                var baseType = rule.GetType().BaseType;
                // ReSharper disable once PossibleNullReferenceException
                var typeFilter = baseType.IsGenericType ? baseType.GetGenericArguments()[0] : typeof(Object);

                foreach (var path in assetPaths)
                {
                    // filter by asset type
                    if (!typeFilter.IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(path))) continue;

                    if (rule.HasIssue(path, out var issue))
                    {
                        _issues.Add(issue);
                    }

                    yield return null;
                }
            }
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
                AnalyzeAssetsByRule(rule, assetPaths);
            }

//            UpdateUI();
        }

        private void AnalyzeAssetsByRule(IAssetRule rule, string[] assetPaths)
        {
            var baseType = rule.GetType().BaseType;
            // ReSharper disable once PossibleNullReferenceException
            var typeFilter = baseType.IsGenericType ? baseType.GetGenericArguments()[0] : typeof(Object);

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

        public void Update()
        {
            var assetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(AllAssetImporter.IsProjectAssetAndNotAFolder)
                .OrderBy(x => x)
                .ToArray();
            AnalyzeAssets(assetPaths);
        }

        public T GetRule<T>()
            where T : IAssetRule
        {
            return (T) _rules.Single(x => x is T);
        }

        public void Dispose()
        {
            AllAssetImporter.AssetPathsChanged -= AllAssetImporterOnAssetPathsChanged;
        }
    }
}
