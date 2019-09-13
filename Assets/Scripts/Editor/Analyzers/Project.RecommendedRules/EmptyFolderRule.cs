using System.IO;
using System.Linq;
using Editor.Issue;
using UnityEditor;

namespace Editor.Analyzers.Project.RecommendedRules
{
    public class EmptyFolderRule : IProjectRule
    {
        public IIssue[] GetIssues()
        {
            return Directory
                .GetDirectories("Assets", "*", SearchOption.AllDirectories)
                .Where(x => Directory.GetFiles(x, "*", SearchOption.AllDirectories).Length == 0)
                .Select(x => (IIssue) new ProjectIssue
                {
                    Message = $"Empty directories shall not exist. Path: \"{x}\"",
                    Type = IssueType.Warning,
                    FixAction = () => Fix(x)
                })
                .ToArray();
        }

        private static bool Fix(string path)
        {
            try
            {
                Directory.Delete(path);
                AssetDatabase.Refresh();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
