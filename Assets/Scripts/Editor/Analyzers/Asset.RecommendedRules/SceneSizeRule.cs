using System.IO;
using UnityEditor;

namespace Editor.Analyzers.Asset.RecommendedRules
{
    public class SceneSizeRule : AssetRule<SceneAsset>
    {
        private readonly int _maxFileSize;

        public SceneSizeRule(AssetAnalyzerSettings settings)
        {
            _maxFileSize = settings.maxFileSize;
        }

        public override bool HasIssue(string path, out AssetIssue<SceneAsset> issue)
        {
            var info = new FileInfo(path);
            if (info.Length > _maxFileSize)
            {
                var expected = (_maxFileSize / 1024f / 1024f).ToString("F2");
                var actual = (info.Length / 1024f / 1024f).ToString("F2");
                issue = new AssetIssue<SceneAsset>(path)
                {
                    Type = AssetIssueType.Warning,
                    Message = $"The file size exceeds the recommended limit of {expected} MB. Actual: {actual} MB"
                };
                return true;
            }

            issue = null;
            return false;
        }
    }
}