using System;
using Editor.Issue;

namespace Editor.Analyzers.Project
{
    public class ProjectIssue : IIssue
    {
        public string Message { get; set; }
        public IssueType Type { get; set; }
        public Func<bool> FixAction { get; set; }
    }
}
