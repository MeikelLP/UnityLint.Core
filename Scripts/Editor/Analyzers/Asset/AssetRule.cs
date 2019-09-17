using UnityEngine;

namespace Editor.Analyzers.Asset
{
    public abstract class AssetRule<T> : IAssetRule<Object>
        where T : Object
    {
        public bool HasIssue(string path, out IAssetIssue<Object> issue)
        {
            issue = null;
            if (HasIssue(path, out AssetIssue<T> newIssue))
            {
                issue = newIssue;
                return true;
            }

            return false;
        }

        public abstract bool HasIssue(string path, out AssetIssue<T> issue);
    }
}
