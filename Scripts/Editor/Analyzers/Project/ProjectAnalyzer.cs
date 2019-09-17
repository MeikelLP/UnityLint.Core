using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Editor.Issue;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Analyzers.Project
{
    public class ProjectAnalyzer : IAnalyzer
    {
        private const string ROW_TEMPLATE_GUID = "824253c8ba9629e47b12df2313279343";
        private const int DELAY_MILLIS = 500;
        private readonly IServiceProvider _provider;
        private readonly List<IIssue> _issues;
        private readonly IProjectRule[] _rules;
        private readonly VisualTreeAsset _rowTemplate;
        private readonly FileSystemWatcher _fileSystemWatcher;

        private DateTime _latestUpdate;
        private bool _isWaiting;

        public int IssueCount => _issues.Count;
        public VisualElement RootElement { get; }

        public ProjectAnalyzer(IServiceProvider provider)
        {
            _provider = provider;
            _issues = new List<IIssue>();
            _fileSystemWatcher = new FileSystemWatcher
            {
                Path = "Assets",
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            _fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
            _fileSystemWatcher.Deleted += FileSystemWatcherOnChanged;
            _fileSystemWatcher.Created += FileSystemWatcherOnChanged;
            _fileSystemWatcher.Renamed += FileSystemWatcherOnChanged;
            _rules = TypeCache.GetTypesDerivedFrom<IProjectRule>()
                .Where(x => !x.IsAbstract)
                .Select(x => ActivatorUtilities.CreateInstance(provider, x))
                .Cast<IProjectRule>()
                .ToArray();
            var rowTemplatePath = AssetDatabase.GUIDToAssetPath(ROW_TEMPLATE_GUID);
            _rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(rowTemplatePath);

            RootElement = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "project-issues"
            };
        }

        private void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            WaitForUpdate();
        }

        private void WaitForUpdate()
        {
            if (_isWaiting)
            {
                _latestUpdate = DateTime.Now;
            }
            else
            {
                _isWaiting = true;
                Task.Run(WaitForUpdateAsync);
            }
        }

        private async Task WaitForUpdateAsync()
        {
            do
            {
                await Task.Delay(DELAY_MILLIS);
            } while (DateTime.Now < _latestUpdate.AddMilliseconds(DELAY_MILLIS));

            LintingEngine.EnqueueOnUnityThread(Update);
            _isWaiting = false;
        }

        public void Dispose()
        {
            _fileSystemWatcher.Changed -= FileSystemWatcherOnChanged;
            _fileSystemWatcher.Deleted -= FileSystemWatcherOnChanged;
            _fileSystemWatcher.Created -= FileSystemWatcherOnChanged;
            _fileSystemWatcher.Renamed -= FileSystemWatcherOnChanged;
            _fileSystemWatcher.Dispose();
        }

        public void Update()
        {
            _issues.Clear();
            var issues = _rules.SelectMany(x => x.GetIssues());
            _issues.AddRange(issues);

            UpdateUI();
        }

        public T GetRule<T>()
            where T : IProjectRule
        {
            return (T) _rules.SingleOrDefault(x => x is T);
        }

        private void UpdateUI()
        {
            RootElement.Clear();

            if (_issues.Count > 0)
            {
                var groups = _issues.GroupBy(x => x.Type).ToArray();

                foreach (var grouping in groups)
                {
                    var container = new VisualElement();
                    var items = grouping.ToArray();
                    var header = IssueUIUtility.GetHeader(grouping.Key, items.Length, container);

                    RootElement.Add(header);
                    RootElement.Add(container);

                    foreach (var issue in items)
                    {
                        var row = _rowTemplate.CloneTree();

                        var text = row.Q<Label>("message");
                        text.text = issue.Message;

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
            else
            {
                var label = new Label("No issues. Well done :)");
                RootElement.Add(label);
            }
        }
    }
}
