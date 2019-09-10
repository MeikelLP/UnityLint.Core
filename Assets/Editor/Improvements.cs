using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Improvements
    {
        [MenuItem("Assets/Copy GUID", validate = true)]
        public static bool CopyGuidValidate()
        {
            return Selection.assetGUIDs.Length > 0;
        }

        [MenuItem("Assets/Copy GUID", priority = 19)]
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
    }
}
