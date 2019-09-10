namespace Editor.Analyzers.Project
{
    public interface IProjectRule
    {
        bool IsValid(ProjectIssue issue);
    }
}
