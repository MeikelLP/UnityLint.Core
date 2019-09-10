using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset
{
    public class AssetIssue
    {
        public string AssetPath { get; }
        public string Message { get; set; }
        public AssetIssueType Type { get; set; }
        public Type AssetType { get; }
        public Object Asset { get; }

        public Action<AssetIssue> Fix { get; set; }

        public AssetIssue(string assetPath)
        {
            AssetPath = assetPath;
            AssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            Asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        }
    }
}
