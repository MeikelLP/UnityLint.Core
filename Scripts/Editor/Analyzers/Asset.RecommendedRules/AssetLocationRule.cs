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
                {".cs", ("Scripts", settings.PrefabPaths.Scripts)},
                {".prefab", ("Prefabs", settings.PrefabPaths.Prefabs)},
                {".mp3", ("Audio files", settings.PrefabPaths.Audio)},
                {".wav", ("Audio files", settings.PrefabPaths.Audio)},
                {".aif", ("Audio files", settings.PrefabPaths.Audio)},
                {".ogg", ("Audio files", settings.PrefabPaths.Audio)},
                {".bmp", ("Images", settings.PrefabPaths.Images)},
                {".tif", ("Images", settings.PrefabPaths.Images)},
                {".tga", ("Images", settings.PrefabPaths.Images)},
                {".png", ("Images", settings.PrefabPaths.Images)},
                {".jpg", ("Images", settings.PrefabPaths.Images)},
                {".jpeg", ("Images", settings.PrefabPaths.Images)},
                {".psd", ("Images", settings.PrefabPaths.Images)},
                {".fbx", ("Models", settings.PrefabPaths.Models)},
                {".blend", ("Models", settings.PrefabPaths.Models)},
                {".c4d", ("Models", settings.PrefabPaths.Models)},
                {".mb", ( "Models", settings.PrefabPaths.Models)},
                {".ma", ( "Models", settings.PrefabPaths.Models)},
                {".dae", ("Models", settings.PrefabPaths.Models)},
                {".obj", ("Models", settings.PrefabPaths.Models)},
                {".dxf", ("Models", settings.PrefabPaths.Models)},
                {".lxo", ("Models", settings.PrefabPaths.Models)},
                {".3ds", ("Models", settings.PrefabPaths.Models)},
                {".jas", ("Models", settings.PrefabPaths.Models)},
                {".unity", ("Scenes", settings.PrefabPaths.Scenes)},
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
