using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset.RecommendedRules
{
    public class AssetLocationRule : AssetRule<Object>
    {
        private readonly Dictionary<string, string> _extensionPathMapping;
        private static readonly Regex AssetPathPrefixReplaceRegex = new Regex("^Assets/", RegexOptions.Compiled);

        public AssetLocationRule(AssetAnalyzerSettings settings)
        {
            _extensionPathMapping = new Dictionary<string, string>
            {
                {".cs", settings.scriptsPath}
            };
        }

        public override bool HasIssue(string path, out AssetIssue<Object> issue)
        {
            issue = null;

            var extension = Path.GetExtension(path);
            switch (extension)
            {
                case ".cs" when !path.StartsWith(_extensionPathMapping[".cs"]):
                    issue = new AssetIssue<Object>(path)
                    {
                        Type = AssetIssueType.Suggestion,
                        Message = $"Scripts should be located under {_extensionPathMapping[".cs"]}"
                    };
                    break;
            }

            if (issue != null)
            {
                issue.FixAction = () => TryFix(path, _extensionPathMapping[extension]);
                return true;
            }

            return false;
        }

        private bool TryFix(string oldPath, string targetPath)
        {
            var newPath = AssetPathPrefixReplaceRegex.Replace(oldPath, "");
            newPath = string.Concat(targetPath.TrimEnd('/'), "/", newPath.TrimStart('/'));
            var targetDir = Path.GetDirectoryName(newPath);
            if (targetDir != null && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                AssetDatabase.Refresh();
            }
            return AssetDatabase.MoveAsset(oldPath, newPath) == "";
        }
    }
}
