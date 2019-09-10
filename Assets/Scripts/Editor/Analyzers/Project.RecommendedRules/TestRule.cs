using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.Analyzers.Project.RecommendedRules
{
    public class TestRule : IProjectRule
    {
        public bool IsValid(ProjectIssue issue)
        {
            var values = Enum.GetValues(typeof(ProjectIssueType));
            issue.Message = "bad";
            var rand = (int)(Random.value * values.Length);
            issue.Type = values.OfType<ProjectIssueType>().ToArray()[rand];
            if (rand > 1)
            {
                issue.Fix = item => { Debug.Log("Fixed " + item.AssetPath); };
            }
            return false;
        }
    }
}
