using Editor.Issue;

namespace Editor.Analyzers.Project
{
    public interface IProjectRule
    {
        IIssue[] GetIssues();
    }
}
