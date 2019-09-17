using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.Assertions;

namespace Editor.Analyzers.Asset
{
    public class AllAssetImporter : AssetPostprocessor
    {
        public static string[] AssetPaths { get; private set; }
        public static event EventHandler<string[]> AssetPathsChanged;

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            AssetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(IsProjectAssetAndNotAFolder)
                .ToArray();
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var list = new List<string>(AssetPaths);
            var moved = movedAssets.Where(IsProjectAssetAndNotAFolder).ToArray();
            var movedFrom = movedFromAssetPaths.Where(IsProjectAssetAndNotAFolder).ToArray();
            var added = importedAssets.Where(IsProjectAssetAndNotAFolder).Except(moved).ToArray();
            var toRemove = deletedAssets.Where(IsProjectAssetAndNotAFolder).ToArray();

            Assert.AreEqual(movedAssets.Length, movedFromAssetPaths.Length);

            // added
            list.AddRange(added);

            // removed
            foreach (var asset in toRemove)
            {
                list.Remove(asset);
            }

            // moved
            for (var i = 0; i < moved.Length; i++)
            {
                var from = movedFrom[i];
                var to = moved[i];

                list.Remove(from);
                list.Add(to);
            }

            // apply
            AssetPaths = list.ToArray();
            AssetPathsChanged?.Invoke(null, AssetPaths);
        }

        public static bool IsProjectAssetAndNotAFolder(string path)
        {
            return path.StartsWith("Assets") &&
                   !Directory.Exists(path);
        }
    }
}
