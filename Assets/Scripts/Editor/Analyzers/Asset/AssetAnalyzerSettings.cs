using System;

namespace Editor.Analyzers.Asset
{
    [Serializable]
    public class AssetAnalyzerSettings : IAnalyzerSettings
    {
        public int maxFileSize = 1024 * 1024 * 2;
    }
}