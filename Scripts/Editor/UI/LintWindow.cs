using System.ComponentModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI
{
    public class LintWindow : EditorWindow
    {
        public const string MAIN_STYLES = "470daf9a6c8a22f4bb4446654b5ffb8e";
        private const string MAIN_UXML = "bf1be80a2b3784446af4c393ce07c6e7";
        private static LintWindow _window;
        private int sidebarIndex = 0;
        private VisualElement _sidebar;
        private VisualElement _main;

        [MenuItem("Window/UnityLint %#F5")]
        public static void Window()
        {
            if (_window != null)
            {
                _window.Close();
            }

            _window = GetWindow<LintWindow>();
            _window.titleContent = new GUIContent("Builder");
            _window.Show();
        }

        private void OnEnable()
        {
            var uxmlPath = AssetDatabase.GUIDToAssetPath(MAIN_UXML);
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            uxml.CloneTree(rootVisualElement);

            var stylePath = AssetDatabase.GUIDToAssetPath(MAIN_STYLES);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath);
            rootVisualElement.styleSheets.Add(styleSheet);

            _main = rootVisualElement.Q<VisualElement>("main");
            _sidebar = rootVisualElement.Q<VisualElement>("sidebar").Q<VisualElement>("analyzers");

            rootVisualElement.Q<Button>("reanalyze-button").clickable = new Clickable(() =>
            {
                for (var i = 0; i < LintingEngine.Analyzers.Length; i++)
                {
                    LintingEngine.Analyzers[i].Update();
                    RefreshSidebar(i);
                }
            });

            for (var i = 0; i < LintingEngine.Analyzers.Length; i++)
            {
                var analyzer = LintingEngine.Analyzers[i];
                var displayName = analyzer.GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ??
                                  analyzer.GetType().Name.Replace("Analyzer", "");
                analyzer.Update();
                var localIndex = i;
                var button = new Button
                {
                    text = $"{displayName} ({analyzer.IssueCount.ToString()})",
                    clickable = new Clickable(() => RefreshSidebar(localIndex))
                };
                _sidebar.Add(button);
            }

            RefreshSidebar(sidebarIndex);
        }

        private void RefreshSidebar(int index)
        {
            var analyzer = LintingEngine.Analyzers[index];
            var sidebarButtons = _sidebar.Query<Button>().ToList();
            for (var i = 0; i < sidebarButtons.Count; i++)
            {
                var sidebarButton = sidebarButtons[i];
                sidebarButton.RemoveFromClassList("is-active");

                if (i == index)
                {
                    sidebarButton.AddToClassList("is-active");
                }
            }
            var displayName = analyzer.GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ??
                              analyzer.GetType().Name.Replace("Analyzer", "");

            sidebarButtons[index].text = $"{displayName} ({analyzer.IssueCount.ToString()})";

            _main.Clear();

            var elem = analyzer.RootElement;
            _main.Add(elem);
            analyzer.Update();
        }
    }
}
