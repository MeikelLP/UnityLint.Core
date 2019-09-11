using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset
{
    public class AssetIssue<T> : IAssetIssue<T> where T : Object
    {
        public string AssetPath { get; }
        public string Message { get; set; }
        public AssetIssueType Type { get; set; }
        public Type AssetType { get; }
        public T Asset { get; }

        public AssetIssue(string assetPath)
        {
            AssetPath = assetPath;
            AssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            Asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
    }
}
