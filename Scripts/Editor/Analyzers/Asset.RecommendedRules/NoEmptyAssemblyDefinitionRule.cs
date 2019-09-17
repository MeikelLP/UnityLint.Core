using System.IO;
using Editor.Issue;
using UnityEditor;
using UnityEditorInternal;

namespace Editor.Analyzers.Asset.RecommendedRules
{
    public class NoEmptyAssemblyDefinitionRule : AssetRule<AssemblyDefinitionAsset>
    {
        public override bool HasIssue(string path, out AssetIssue<AssemblyDefinitionAsset> issue)
        {
            issue = null;

            var dir = Path.GetDirectoryName(path);
            if (dir == null) return false;

            var files = Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                issue = new AssetIssue<AssemblyDefinitionAsset>(path)
                {
                    Message = "Assembly definition files shall not be empty.",
                    Type = IssueType.Warning,
                    FixAction = () => AssetDatabase.DeleteAsset(path)
                };
                return true;
            }

            return false;
        }
    }
}
