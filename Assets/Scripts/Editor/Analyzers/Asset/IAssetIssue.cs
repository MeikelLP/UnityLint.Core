using System;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset
{
    public interface IAssetIssue<out T> where T : Object
    {
        string AssetPath { get; }
        string Message { get; set; }
        AssetIssueType Type { get; set; }
        Type AssetType { get; }
        T Asset { get; }
    }
}
