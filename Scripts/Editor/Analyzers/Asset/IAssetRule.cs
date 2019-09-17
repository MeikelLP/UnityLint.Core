using UnityEngine;

namespace Editor.Analyzers.Asset
{
    public interface IAssetRule
    {
        bool HasIssue(string path, out IAssetIssue<Object> issue);
    }

    public interface IAssetRule<T> : IAssetRule
        where T : Object
    {
        bool HasIssue(string path, out IAssetIssue<T> issue);
    }
}
