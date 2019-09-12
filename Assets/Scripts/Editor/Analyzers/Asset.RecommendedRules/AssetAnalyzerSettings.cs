using System;

namespace Editor.Analyzers.Asset.RecommendedRules
{
    [Serializable]
    public class AssetAnalyzerSettings : IAnalyzerSettings
    {
        public int maxFileSize = 1024 * 1024 * 2;
        public string scriptsPath = "Assets/Scripts";
    }
}
