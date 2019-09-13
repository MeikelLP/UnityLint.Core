using System;

namespace Editor.Issue
{
    public interface IIssue
    {
        string Message { get; }
        IssueType Type { get; }
        Func<bool> FixAction { get; }
    }
}
