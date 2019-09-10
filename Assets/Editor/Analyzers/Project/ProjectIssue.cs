using System;

namespace Editor.Analyzers.Project
{
    public class ProjectIssue
    {
        public string AssetPath { get; set; }
        public string Message { get; set; }
        public Type Type { get; set; }
    }
}
