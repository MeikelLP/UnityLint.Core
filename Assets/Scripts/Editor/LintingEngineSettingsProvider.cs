using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Extensions;
using Editor.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [InitializeOnLoad]
    public static class LintingEngineSettingsProvider
    {
        public const string SETTINGS_ASSET_PATH = "ProjectSettings/UnityLint.json";
        public static UnityLintSettings Settings { get; }

        static LintingEngineSettingsProvider()
        {
            EditorApplication.quitting += SaveSettings;
            var analyzerSettingTypes = TypeCache.GetTypesDerivedFrom<IAnalyzerSettings>().ToArray();

            if (File.Exists(SETTINGS_ASSET_PATH))
            {
                var json = File.ReadAllText(SETTINGS_ASSET_PATH);
                try
                {
                    Settings = DeserializeFromString(json);

                    var toAdd = analyzerSettingTypes
                        .Where(x => !Settings.AnalyzerSettings.Keys.Contains(x.AssemblyQualifiedName))
                        .ToArray();
                    foreach (var type in toAdd)
                    {
                        if (type.AssemblyQualifiedName == null) continue;
                        Settings.AnalyzerSettings.Add(type.AssemblyQualifiedName, (IAnalyzerSettings) Activator.CreateInstance(type));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError("Failed to use \"" + SETTINGS_ASSET_PATH +
                                   "\". Overwriting with default settings file.");
                }
            }

            if (Settings == null)
            {
                Settings = new UnityLintSettings
                {
                    AnalyzerSettings = analyzerSettingTypes
                        .Select(Activator.CreateInstance)
                        .Cast<IAnalyzerSettings>()
                        .ToDictionary(k => k.GetType().AssemblyQualifiedName, v => v)
                };
                SaveSettings();
            }
        }

        private static void SaveSettings()
        {
            var str = SerializeToString(Settings);
            File.WriteAllText(SETTINGS_ASSET_PATH, str);
        }

        private static string SerializeToString(UnityLintSettings obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        private static UnityLintSettings DeserializeFromString(string str)
        {
            var jObj = JObject.Parse(str);
            var prop = jObj.Property(nameof(UnityLintSettings.AnalyzerSettings));
            if (prop == null) return null;

            var dict = prop.Value.ToObject<JObject>();
            var settings = new UnityLintSettings
            {
                AnalyzerSettings = new Dictionary<string, IAnalyzerSettings>()
            };

            foreach (var (key, value) in dict)
            {
                try
                {
                    var type = Type.GetType(key);
                    if (type == null) continue;
                    var jObject = value.ToObject(type);

                    settings.AnalyzerSettings.Add(key, (IAnalyzerSettings) jObject);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogError($"Could not load settings object \"{key}\". Skipping.");
                }
            }

            return settings;
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Project/UnityLint", SettingsScope.Project)
            {
                label = "UnityLint",
                activateHandler = (searchContext, rootElement) =>
                {
                    var properties = new VisualElement
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    foreach (var (key, value) in Settings.AnalyzerSettings)
                    {
                        var type = Type.GetType(key);
                        if (type == null) continue;
                        var label = new Label(type.Name);
                        label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
                        properties.Add(label);
                        foreach (var fieldInfo in value.GetType().GetFields())
                        {
                            var field = UIUtility.GetField(value, fieldInfo);
                            properties.Add(field);
                        }
                    }

                    var button = new Button(SaveSettings)
                    {
                        text = "Save changes now to file",
                        tooltip =
                            "Changes won't be saved to file if Unity crashes. You can use this to safe them to file."
                    };
                    rootElement.Add(button);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] {"Unity", "Linting", "Lint"})
            };

            return provider;
        }
    }
}
