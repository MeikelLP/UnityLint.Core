using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine.Assertions;

namespace Editor.Analyzers.Scripting
{
    public class UnityCsprojUpdater : IDisposable
    {
        private readonly ScriptingAnalyzer _analyzer;

        public UnityCsprojUpdater(ScriptingAnalyzer analyzer)
        {
            _analyzer = analyzer;

            // update csproj if analyzers are added or removed to keep the csproj up to date
            // even if the csproj is not newly generated
            _analyzer.AnalyzersChanged += AnalyzerOnAnalyzersChanged;
        }

        private static void AnalyzerOnAnalyzersChanged(object sender, string[] analyzers)
        {
            var projects = Directory.GetFiles(".", "*.csproj");

            foreach (var path in projects)
            {
                var content = File.ReadAllText(path);
                content = UpdateCsProject(analyzers, content);
                File.WriteAllText(path, content);
            }
        }

        public static string UpdateCsProject(string[] analyzers, string content)
        {
            var doc = XDocument.Parse(content);
            var root = doc.Root;

            Assert.IsNotNull(root, "doc.Root != null");

            XNamespace xmlns = root.Name.NamespaceName; // don't use var

            var grp = root.Elements(xmlns + "ItemGroup").FirstOrDefault();
            if (grp == null)
            {
                grp = new XElement(xmlns + "ItemGroup");
                root.Add(grp);
            }

            var existingAnalyzers = root.Descendants(xmlns + "Analyzer").ToList();
            foreach (var element in existingAnalyzers)
            {
                var attr = element.Attribute("Include")?.Value;
                if (attr == null) continue;
                if (!analyzers.Contains(attr))
                {
                    element.Remove();
                }
            }

            foreach (var analyzerAssembly in analyzers)
            {
                var analyzer = new XElement(xmlns + "Analyzer");
                analyzer.SetAttributeValue("Include", analyzerAssembly);
                grp.AddFirst(analyzer);
            }

            using (var sw = new StringWriter())
            {
                doc.Save(sw);
                return sw.ToString();
            }
        }

        public void Dispose()
        {
            _analyzer.AnalyzersChanged -= AnalyzerOnAnalyzersChanged;
        }
    }
}
