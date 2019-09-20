using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Issue
{
    public static class IssueUIUtility
    {
        public const string BUILT_IN_ASSET_PATH = "Library/unity editor resources";
        private const string HEADER_UXML_GUID = "2f7c085e117c42ac99abfa2f7613b201";
        private static readonly VisualTreeAsset HeaderTemplate;


        private static readonly Dictionary<IssueType, long> BuildInIconsForIssueTypes = new Dictionary<IssueType, long>
        {
            {IssueType.Info, 5425037494185492166},
            {IssueType.Suggestion, -4603091085154494538},
            {IssueType.Warning, -5763820162405496800},
            {IssueType.Error, -2005373149481181617},
        };

        static IssueUIUtility()
        {
            var headerTemplatePath = AssetDatabase.GUIDToAssetPath(HEADER_UXML_GUID);
            HeaderTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(headerTemplatePath);
        }

        public static VisualElement GetHeader(IssueType type, int length, VisualElement container)
        {
            var header = HeaderTemplate.CloneTree()[0];
            header.name = $"header-{type}";
            var image = header.Q<Image>("icon");
            var heading = header.Q<Button>("heading");


            image.image = type.ToIcon();
            heading.text = $"{type.ToString()} ({length.ToString()})";
            heading.clickable = new Clickable(() =>
            {
                if (container.GetClasses().Contains("is-hidden"))
                {
                    container.RemoveFromClassList("is-hidden");
                }
                else
                {
                    container.AddToClassList("is-hidden");
                }
            });

            return header;
        }


        public static Texture2D ToIcon(this IssueType type)
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
