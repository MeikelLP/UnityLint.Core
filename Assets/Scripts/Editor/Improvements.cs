using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Improvements
    {
        [MenuItem("Assets/Copy/GUID", validate = true)]
        public static bool CopyGuidValidate()
        {
            return Selection.assetGUIDs.Length > 0;
        }

        [MenuItem("Assets/Copy/GUID", priority = 19)]
        public static void CopyGuid()
        {
            var guids = Selection.assetGUIDs;

            var editor = new TextEditor
            {
                text = string.Join("\n", guids)
            };

            editor.SelectAll();
            editor.Copy();
        }

        [MenuItem("Assets/Copy/Asset Type", validate = true)]
        public static bool CopyAssetTypeValidate()
        {
            return Selection.assetGUIDs.Length == 1;
        }

        [MenuItem("Assets/Copy/Asset Type", priority = 19)]
        public static void CopyAssetType()
        {
            var guid = Selection.assetGUIDs[0];

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var editor = new TextEditor
            {
                text = AssetDatabase.GetMainAssetTypeAtPath(path).Name
            };

            editor.SelectAll();
            editor.Copy();
        }
    }
}
