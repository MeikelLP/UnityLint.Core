using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine.UIElements;

namespace Editor.Analyzers
{
    public class ScriptingAnalyzer : IAnalyzer
    {
        private const string LOG_FILE_EXTENSIONS = ".json";
        private static readonly string LogDirectory = Path.Combine("Temp", "Compilation");
        private string[] _assemblyNames;
        private int navIndex;

        public (string File, CompilerMessage[] Messages)[] Assemblies { get; private set; }
        public int IssueCount => Assemblies.Sum(x => x.Messages.Length);

        public ScriptingAnalyzer()
        {
            Assemblies = Array.Empty<(string, CompilerMessage[])>();

            CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
            CompilationPipeline.compilationStarted += CompilationPipelineOnCompilationStarted;


            Task.Run(LoadMessagesAsync);
        }

        private async Task LoadMessagesAsync()
        {
            var files = Directory.GetFiles(LogDirectory, "*" + LOG_FILE_EXTENSIONS);
            var jsons = await GetJsons<CompilerMessage[]>(files);
            Assemblies = new (string File, CompilerMessage[] Messages)[jsons.Length];
            for (var i = 0; i < jsons.Length; i++)
            {
                var (file, json) = jsons[i];

                var path = Path.GetFileNameWithoutExtension(file) ?? throw new ArgumentNullException(nameof(file));
                Assemblies[i] = (path, json);

                if (json.Length == 0)
                {
                    File.Delete(file);
                }
            }
        }

        private static async Task<(string File, T Json)> GetJson<T>(string file)
        {
            using (var reader = new StreamReader(file))
            {
                var jsonText = await reader.ReadToEndAsync().ConfigureAwait(false);
                var json = JsonConvert.DeserializeObject<T>(jsonText);
                return (file, json);
            }
        }

        private static async Task<(string File, T Json)[]> GetJsons<T>(string[] files)
        {
            var tasks = new Task<(string, T)>[files.Length];
            for (var i = 0; i < files.Length; i++)
            {
                tasks[i] = GetJson<T>(files[i]);
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private void CompilationPipelineOnCompilationStarted(object obj)
        {
            var assemblyDefinitions = AssetDatabase
                .FindAssets("t:AssemblyDefinitionAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(x => x.StartsWith("Assets/"))
                .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                .ToArray();
            _assemblyNames = assemblyDefinitions.Select(x => $"{x.name}.dll").Concat(new[]
            {
                "Assembly-CSharp.dll",
                "Assembly-CSharp-Editor.dll",
                "Assembly-CSharp-firstpass.dll",
                "Assembly-CSharp-Editor-firstpass.dll"
            }).ToArray();
            if (!Directory.Exists(LogDirectory)) Directory.CreateDirectory(LogDirectory);
        }

        private void CompilationPipelineOnAssemblyCompilationFinished(string assemblyName,
            CompilerMessage[] messages)
        {
            if (!_assemblyNames.Contains(Path.GetFileName(assemblyName)))
            {
                return;
            }

            var path = Path.Combine(LogDirectory, Path.GetFileNameWithoutExtension(assemblyName) + LOG_FILE_EXTENSIONS);
            File.WriteAllText(path, JsonConvert.SerializeObject(messages));
        }

        public void GetVisualElement(VisualElement parent)
        {
            var nav = new VisualElement {name = "nav"};
            var items = new VisualElement {name = "items"};
            parent.Add(nav);
            parent.Add(items);

            for (var i = 0; i < Assemblies.Length; i++)
            {
                var (key, message) = Assemblies[i];
                var elem = new Button
                {
                    text = $"{key} ({message.Length.ToString()})"
                };
                var localIndex = i;
                elem.clickable = new Clickable(() =>
                {
                    navIndex = localIndex;
                    RefreshList(navIndex, items, nav);
                });
                nav.Add(elem);
            }

            RefreshList(navIndex, items, nav);
        }

        private void RefreshList(int index, VisualElement container, VisualElement nav)
        {
            var messages = Assemblies[index].Messages;
            DrawAssemblyMessages(container, messages);
            var buttons = nav.Query<Button>().ToList();
            for (var i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                button.RemoveFromClassList("is-active");
                if (i == index)
                {
                    button.AddToClassList("is-active");
                }
            }
        }

        private void DrawAssemblyMessages(VisualElement itemsContainer,
            CompilerMessage[] messages)
        {
            itemsContainer.Clear();

            foreach (var msg in messages)
            {
                var btn = new Button
                {
                    text = msg.message,
                    clickable = new Clickable(() =>
                    {
                        var file = AssetDatabase.LoadAssetAtPath<MonoScript>(msg.file);
                        AssetDatabase.OpenAsset(file);
                    })
                };
                itemsContainer.Add(btn);
            }
        }

        public void Dispose()
        {
            CompilationPipeline.assemblyCompilationFinished -= CompilationPipelineOnAssemblyCompilationFinished;
            CompilationPipeline.compilationStarted -= CompilationPipelineOnCompilationStarted;
        }
    }
}
