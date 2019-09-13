using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Editor.Issue;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Editor.Analyzers.Asset.RecommendedRules
{
    public class AssetLocationRule : AssetRule<Object>
    {
        private readonly Dictionary<string, (string Label, string RecommendedPath)> _extensionPathMapping;
        private static readonly Regex AssetPathPrefixReplaceRegex = new Regex("^Assets/", RegexOptions.Compiled);

        public AssetLocationRule(AssetAnalyzerSettings settings)
        {
            _extensionPathMapping = new Dictionary<string, (string Label, string RecommendedPath)>
            {
                {".cs", ("Scripts", settings.scriptsPath)},
                {".prefab", ("Prefabs", settings.prefabsPath)},
                {".mp3", ("Audio files", settings.audioPath)},
                {".wav", ("Audio files", settings.audioPath)},
                {".aif", ("Audio files", settings.audioPath)},
                {".ogg", ("Audio files", settings.audioPath)},
                {".bmp", ("Images", settings.imagesPath)},
                {".tif", ("Images", settings.imagesPath)},
                {".tga", ("Images", settings.imagesPath)},
                {".png", ("Images", settings.imagesPath)},
                {".jpg", ("Images", settings.imagesPath)},
                {".jpeg", ("Images", settings.imagesPath)},
                {".psd", ("Images", settings.imagesPath)},
                {".fbx", ("Models", settings.modelsPath)},
                {".blend", ("Models", settings.modelsPath)},
                {".c4d", ("Models", settings.modelsPath)},
                {".mb", ( "Models", settings.modelsPath)},
                {".ma", ( "Models", settings.modelsPath)},
                {".dae", ("Models", settings.modelsPath)},
                {".obj", ("Models", settings.modelsPath)},
                {".dxf", ("Models", settings.modelsPath)},
                {".lxo", ("Models", settings.modelsPath)},
                {".3ds", ("Models", settings.modelsPath)},
                {".jas", ("Models", settings.modelsPath)},
                {".unity", ("Scenes", settings.scenesPath)},
            };
        }

        public override bool HasIssue(string path, out AssetIssue<Object> issue)
        {
            issue = null;

            const IssueType issueType = IssueType.Suggestion;
            const string messageFormat = "{0} should be located under {1}";

            var extension = Path.GetExtension(path)?.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(extension) &&
                _extensionPathMapping.TryGetValue(extension, out var pair) &&
                !path.StartsWith(pair.RecommendedPath))
            {
                issue = new AssetIssue<Object>(path)
                {
                    Type = issueType,
                    Message = string.Format(messageFormat, pair.Label, pair.RecommendedPath),
                    FixAction = () => TryFix(path, pair.RecommendedPath)
                };
                return true;
            }


            return false;
        }

        private static bool TryFix(string oldPath, string targetPath)
        {
            // move files to dir/asset to $targetPath/dir/asset
            var newPath = AssetPathPrefixReplaceRegex.Replace(oldPath, "");
            newPath = string.Concat(targetPath.TrimEnd('/'), "/", newPath.TrimStart('/'));
            var targetDir = Path.GetDirectoryName(newPath);
            if (targetDir != null && !Directory.Exists(targetDir))
            {
                // ensure target dir exists
                Directory.CreateDirectory(targetDir);
                AssetDatabase.Refresh();
            }
            return AssetDatabase.MoveAsset(oldPath, newPath) == "";
        }
    }
}
