using UnityEngine.UIElements;

namespace Editor.Analyzers
{
    public class ProjectAnalyzer : IAnalyzer
    {
        public void Dispose()
        {

        }

        public int IssueCount { get; }
        public void GetVisualElement(VisualElement parent)
        {

            parent.Add(new Label("TODO"));
        }
    }
}
