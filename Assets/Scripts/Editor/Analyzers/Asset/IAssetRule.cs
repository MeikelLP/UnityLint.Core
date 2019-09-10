namespace Editor.Analyzers.Asset
{
    public interface IAssetRule
    {
        bool IsValid(AssetIssue issue);
    }
}
