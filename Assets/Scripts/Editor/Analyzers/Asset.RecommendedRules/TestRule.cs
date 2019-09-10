namespace Editor.Analyzers.Asset.RecommendedRules
{
    public class TestRule : IAssetRule
    {
        public bool IsValid(AssetIssue issue)
        {
            return true;
        }
    }
}
