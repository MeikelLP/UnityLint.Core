using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Editor.Analyzers.Project;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Extensions
{
    public static class UnityExtensions
    {
        private const string BUILT_IN_ASSET_PATH = "Library/unity editor resources";

        /// <summary>
        /// These GUIDs seem to be safe. For example <see cref="AssemblyDefinitionAsset"/> has the same GUID in 2019.2
        /// and 2019.3 (with new UI). It's safe to assume these values won't change.
        /// </summary>
        private static readonly ReadOnlyDictionary<Type, long> BuildInIconsForTypes = new ReadOnlyDictionary<Type, long>(
            new Dictionary<Type, long>
            {
                {typeof(AssemblyDefinitionAsset), -5767812303953593571},
                {typeof(MonoScript), 8647890191352912404},
                {typeof(GameObject), -6840528455795640641},
                {typeof(SceneAsset), -4890957673588117743},
                {typeof(StyleSheet), 5153532147187264368},
                {typeof(TextAsset), 1831301468445745894},
                {typeof(Texture2D), 2964569609108149060},
                {typeof(VisualTreeAsset), -1113042662600692791},
            });

        private static readonly ReadOnlyDictionary<ProjectIssueType, long> BuildInIconsForIssueTypes = new ReadOnlyDictionary<ProjectIssueType, long>(
            new Dictionary<ProjectIssueType, long>
            {
                {ProjectIssueType.Info, 5425037494185492166},
                {ProjectIssueType.Suggestion, -4603091085154494538},
                {ProjectIssueType.Warning, -5763820162405496800},
                {ProjectIssueType.Error, -2005373149481181617},
            });

        public static Texture2D ToIcon(this Type type)
        {
            if (BuildInIconsForTypes.TryGetValue(type, out var localId))
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(BUILT_IN_ASSET_PATH);
                foreach (var asset in assets)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long id) &&
                        id == localId &&
                        asset is Texture2D texture2D)
                    {
                        return texture2D;
                    }
                }
            }

            return Texture2D.whiteTexture;
        }

        public static Texture2D ToIcon(this ProjectIssueType type)
        {
            if (BuildInIconsForIssueTypes.TryGetValue(type, out var localId))
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(BUILT_IN_ASSET_PATH);
                foreach (var asset in assets)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long id) &&
                        id == localId &&
                        asset is Texture2D texture2D)
                    {
                        return texture2D;
                    }
                }
            }

            return Texture2D.whiteTexture;
        }
    }
}
