namespace Editor.Analyzers.Project.RecommendedRules
{
    public class TestRule : IProjectRule
    {
        public string Validate(string path)
        {
            return "bad";
        }
    }
}
