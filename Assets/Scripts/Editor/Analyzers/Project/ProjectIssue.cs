using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Project
{
    public class ProjectIssue
    {
        public string AssetPath { get; }
        public string Message { get; set; }
        public ProjectIssueType Type { get; set; }
        public Type AssetType { get; }
        public Object Asset { get; }

        public Action<ProjectIssue> Fix { get; set; }

        public ProjectIssue(string assetPath)
        {
            AssetPath = assetPath;
            AssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            Asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        }
    }
}
