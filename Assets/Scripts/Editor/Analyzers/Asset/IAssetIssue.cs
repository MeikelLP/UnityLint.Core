using System;
using Editor.Issue;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset
{
    public interface IAssetIssue<out T> : IIssue
        where T : Object
    {
        string AssetPath { get; }
        Type AssetType { get; }
        T Asset { get; }
    }
}
