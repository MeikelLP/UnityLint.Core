using System;

namespace Editor.Analyzers.Asset.RecommendedRules
{
    [Serializable]
    public class AssetAnalyzerSettings : IAnalyzerSettings
    {
        public int maxFileSize = 1024 * 1024 * 2;
        public string scriptsPath = "Assets/Scripts";
        public string prefabsPath = "Assets/Prefabs";
        public string audioPath = "Assets/Audio";
        public string imagesPath = "Assets/Images";
        public string modelsPath = "Assets/Models";
        public string scenesPath = "Assets/Scenes";
    }
}
