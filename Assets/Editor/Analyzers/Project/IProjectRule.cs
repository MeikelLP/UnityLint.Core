namespace Editor.Analyzers.Project
{
    public interface IProjectRule
    {
        string Validate(string path);
    }
}
