using System;
using UnityEngine.UIElements;

namespace Editor
{
    public interface IAnalyzer : IDisposable
    {
        int IssueCount { get; }
        VisualElement RootElement { get; }
        void Update();
    }
}
