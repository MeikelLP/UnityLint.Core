using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine.UIElements;

namespace Editor.Analyzers.Scripting
{
    public class ScriptingAnalyzer : IAnalyzer
    {
        public const string UNITY_CODE_ANALYSIS_PACKAGE_NAME = "com.unity.code-analysis";
        public const string ANALYZERS_DIR = "Analyzers";
        private const string LOG_FILE_EXTENSIONS = ".json";
        private static readonly string LogDirectory = Path.Combine("Temp", "Compilation");
        private string[] _assemblyNames;
        private int _navIndex;
        private FileSystemWatcher _analyzersWatcher;
        private readonly FileSystemWatcher _packageManifestWatcher;
        private readonly FileSystemWatcher _rootWatcher;
        private readonly CscUpdater _cscUpdater;
        private readonly UnityCsprojUpdater _unityCsprojUpdater;
        public event EventHandler<string[]> AnalyzersChanged;

        public (string File, CompilerMessage[] Messages)[] ScriptAssemblies { get; private set; }
        public string[] AnalyzerAssemblies { get; private set; }
        public int IssueCount => ScriptAssemblies.Sum(x => x.Messages.Length);
        public VisualElement RootElement { get; }

        public string UnityCodeAnalysisPackagePath
        {
            get
            {
                var codeAnalysisDirectory = Directory
                    .GetDirectories(Path.Combine("Library", "PackageCache"), $"{UNITY_CODE_ANALYSIS_PACKAGE_NAME}*")
                    .FirstOrDefault();
                return codeAnalysisDirectory == null
                    ? null
                    : Path.Combine(codeAnalysisDirectory, "Plugins", "Microsoft.CodeAnalysis.dll");
            }
        }

        public ScriptingAnalyzer()
        {
            ScriptAssemblies = Array.Empty<(string, CompilerMessage[])>();

            CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
            CompilationPipeline.compilationStarted += CompilationPipelineOnCompilationStarted;

            _rootWatcher = new FileSystemWatcher(".")
            {
                NotifyFilter = NotifyFilters.Attributes,
                EnableRaisingEvents = true
            };
            _rootWatcher.Created += RootWatcherOnUpdate;
            _rootWatcher.Deleted += RootWatcherOnUpdate;
            _packageManifestWatcher = new FileSystemWatcher("Packages")
            {
                Filter = "manifest.json",
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            _packageManifestWatcher.Changed += AnalyzersWatcherOnUpdate;

            ToggleAnalyzersWatcher(Directory.Exists(ANALYZERS_DIR));
            _cscUpdater = new CscUpdater(this);
            _unityCsprojUpdater = new UnityCsprojUpdater(this);

            var uxmlPath = AssetDatabase.GUIDToAssetPath("acdfad26f5084a85addd60860e6e78d3");
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            RootElement = uxml.CloneTree();

            ScanForAnalyzers();
        }

        private void ToggleAnalyzersWatcher(bool toggle)
        {
            if (toggle)
            {
                _analyzersWatcher = new FileSystemWatcher(ANALYZERS_DIR)
                {
                    Filter = "*.dll",
                    NotifyFilter = NotifyFilters.LastWrite |
                                   NotifyFilters.FileName |
                                   NotifyFilters.DirectoryName |
                                   NotifyFilters.Attributes,
                    EnableRaisingEvents = true
                };
                _analyzersWatcher.Created += AnalyzersWatcherOnUpdate;
                _analyzersWatcher.Deleted += AnalyzersWatcherOnUpdate;
            }
            else
            {
                if (_analyzersWatcher != null)
                {
                    _analyzersWatcher.Created -= AnalyzersWatcherOnUpdate;
                    _analyzersWatcher.Deleted -= AnalyzersWatcherOnUpdate;
                    _analyzersWatcher.Dispose();
                    _analyzersWatcher = null;
                }
            }
        }

        private void RootWatcherOnUpdate(object sender, FileSystemEventArgs e)
        {
            // check if is directory
            if ((File.GetAttributes(e.FullPath) & FileAttributes.Directory) == 0) return;

            // only enable file watcher if the directory exists
            if ((e.ChangeType & WatcherChangeTypes.Created) != 0)
            {
                ToggleAnalyzersWatcher(true);
            }
            else if((e.ChangeType & WatcherChangeTypes.Deleted) != 0 || (e.ChangeType & WatcherChangeTypes.Renamed) != 0)
            {
                ToggleAnalyzersWatcher(false);
            }
        }

        private void AnalyzersWatcherOnUpdate(object sender, FileSystemEventArgs e)
        {
            LintingEngine.EnqueueOnUnityThread(ScanForAnalyzers);
            LintingEngine.EnqueueOnUnityThread(CompilationPipeline.RequestScriptCompilation);
        }

        private async Task LoadMessagesAsync()
        {
            var files = Directory.GetFiles(LogDirectory, "*" + LOG_FILE_EXTENSIONS);
            var jsons = await GetJsons<CompilerMessage[]>(files);
            ScriptAssemblies = new (string File, CompilerMessage[] Messages)[jsons.Length];
            for (var i = 0; i < jsons.Length; i++)
            {
                var (file, json) = jsons[i];

                var path = Path.GetFileNameWithoutExtension(file) ?? throw new ArgumentNullException(nameof(file));
                ScriptAssemblies[i] = (path, json);
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

        public void Update()
        {
            Task.Run(LoadMessagesAsync).Wait();
            if (UnityCodeAnalysisPackagePath == null)
            {
                var warnLabel =
                    new Label($"You don't have the \"{UNITY_CODE_ANALYSIS_PACKAGE_NAME}\" package installed.");
                RootElement.Add(warnLabel);
            }

            var nav = RootElement[0];
            var items = RootElement[1];

            nav.Clear();

            for (var i = 0; i < ScriptAssemblies.Length; i++)
            {
                var (key, message) = ScriptAssemblies[i];
                var elem = new Button
                {
                    text = $"{key} ({message.Length.ToString()})"
                };
                var localIndex = i;
                elem.clickable = new Clickable(() =>
                {
                    _navIndex = localIndex;
                    RefreshList(_navIndex, items, nav);
                });
                nav.Add(elem);
            }

            if (ScriptAssemblies.Length > 0)
            {
                RefreshList(_navIndex, items, nav);
            }
        }


        private void RefreshList(int index, VisualElement items, VisualElement nav)
        {
            var messages = ScriptAssemblies[index].Messages;
            DrawAssemblyMessages(items, messages);
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

        private static void DrawAssemblyMessages(VisualElement itemsContainer,
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

            if (_analyzersWatcher != null)
            {
                _analyzersWatcher.Created -= AnalyzersWatcherOnUpdate;
                _analyzersWatcher.Deleted -= AnalyzersWatcherOnUpdate;
                _analyzersWatcher.Dispose();
            }

            if (_rootWatcher != null)
            {
                _rootWatcher.Created -= RootWatcherOnUpdate;
                _rootWatcher.Deleted -= RootWatcherOnUpdate;
                _rootWatcher.Dispose();
            }

            _packageManifestWatcher.Changed -= AnalyzersWatcherOnUpdate;
            _packageManifestWatcher.Dispose();
            _cscUpdater.Dispose();
            _unityCsprojUpdater.Dispose();
        }

        public void ScanForAnalyzers()
        {
            var oldAnalyzers = AnalyzerAssemblies;
            var newAnalyzers = Directory.Exists(ANALYZERS_DIR)
                ? Directory.GetFiles(ANALYZERS_DIR, "*.dll")
                : Array.Empty<string>();


            if (oldAnalyzers == null || !oldAnalyzers.SequenceEqual(newAnalyzers))
            {
                AnalyzerAssemblies = newAnalyzers;

                AnalyzersChanged?.Invoke(this, newAnalyzers);
            }
        }
    }
}
