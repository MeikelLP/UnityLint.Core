using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Editor.Issue;
using UnityEngine;

namespace Editor.Analyzers.Asset.RecommendedRules
{
    public class NamingConventionsRule : AssetRule<Object>
    {
        private readonly Dictionary<string, NamingConvention> _conventions;
        public static readonly Dictionary<NamingConvention, Regex> ConventionValidator =
            new Dictionary<NamingConvention, Regex>
        {
            {NamingConvention.AllCaps, new Regex("[A-Z\\d_]+$", RegexOptions.Compiled)},
            {NamingConvention.LowerCamelCase, new Regex("^[a-z]{1}([a-z0-9]+|([A-Z]{1,2}[a-z0-9]+))*$", RegexOptions.Compiled)},
            {NamingConvention.UpperCamelCase, new Regex("^([A-Z]{1,2}?[a-z0-9]+)*$", RegexOptions.Compiled)},
            {NamingConvention.SnakeCase, new Regex("^[a-z]+[_a-z0-9]*[a-z0-9]$", RegexOptions.Compiled)},
        };

        public NamingConventionsRule(AssetAnalyzerSettings settings)
        {
            _conventions = new Dictionary<string, NamingConvention>
            {
                {".cs", settings.Conventions.Scripts},
                {".blend", settings.Conventions.Models},
                {".test1", NamingConvention.AllCaps},
                {".test2", NamingConvention.SnakeCase},
                {".test3", NamingConvention.LowerCamelCase},
            };
        }

        public override bool HasIssue(string path, out AssetIssue<Object> issue)
        {
            issue = null;
            var extensions = Path.GetExtension(path);
            var filename = Path.GetFileNameWithoutExtension(path);
            if (extensions != null &&
                filename != null &&
                _conventions.TryGetValue(extensions, out var convention))
            {
                if (!ConventionValidator[convention].IsMatch(filename))
                {
                    issue = new AssetIssue<Object>(path)
                    {
                        Message = $"The filename does not match the convention: {convention.ToString()}",
                        Type = IssueType.Suggestion
                    };
                    return true;
                }
            }
            return false;
        }
    }
}
