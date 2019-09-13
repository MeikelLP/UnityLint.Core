using System;
using Editor.Issue;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset
{
    public class AssetIssue<T> : IAssetIssue<T> where T : Object
    {
        public string AssetPath { get; }
        public string Message { get; set; }
        public IssueType Type { get; set; }
        public Type AssetType { get; }
        public T Asset { get; }
        public Func<bool> FixAction { get; set; }

        public AssetIssue(string assetPath)
        {
            AssetPath = assetPath;
            AssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            Asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
    }
}
