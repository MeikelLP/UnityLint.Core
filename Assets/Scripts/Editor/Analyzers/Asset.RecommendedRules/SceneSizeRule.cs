using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Analyzers.Asset.RecommendedRules
{
    public class SceneSizeRule : AssetRule<SceneAsset>
    {
        private const int MAX_FILE_SIZE = 1024 * 1024 * 2;

        public override bool HasIssue(string path, out AssetIssue<SceneAsset> issue)
        {
            var info = new FileInfo(path);
            if (info.Length > MAX_FILE_SIZE)
            {
                var expected = (MAX_FILE_SIZE / 1024f / 1024f).ToString("F2");
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
