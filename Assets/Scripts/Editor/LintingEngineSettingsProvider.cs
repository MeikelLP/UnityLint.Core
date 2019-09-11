using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.Analyzers.Asset;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor
{
    [InitializeOnLoad]
    public static class LintingEngineSettingsProvider
    {
        public const string SETTINGS_ASSET_PATH = "ProjectSettings/UnityLint.asset";
        public static UnityLintSettings Settings { get; }

        static LintingEngineSettingsProvider()
        {
            if (File.Exists(SETTINGS_ASSET_PATH))
            {
                var json = File.ReadAllText(SETTINGS_ASSET_PATH);
                Settings = DeserializeFromString(json);
            }

            if (Settings == null)
            {
                Settings = new UnityLintSettings
                {
                    Settings = new IAnalyzerSettings[]
                    {
                        new AssetAnalyzerSettings()
                    }
                };
                var str = SerializeToString(Settings);
                File.WriteAllText(SETTINGS_ASSET_PATH, str);
            }
        }

        private static string SerializeToString(UnityLintSettings obj)
        {
            var jObject = JObject.FromObject(obj);
            var prop = jObject.Property(nameof(UnityLintSettings.Settings));
            var array = prop.Values().ToArray();
            for (var i = 0; i < array.Length; i++)
            {
                var jToken = array[i];
                var settingsObj = jToken.Value<JObject>();
                var typeName = Settings.Settings[i].GetType().FullName;
                settingsObj.Add("#type", typeName);
            }

            return jObject.ToString();
        }

        private static UnityLintSettings DeserializeFromString(string str)
        {
            var jObj = JObject.Parse(str);
            var prop = jObj.Property(nameof(UnityLintSettings.Settings));
            if (prop == null) return null;

            var array = prop.Values().ToArray();
            var settings = new UnityLintSettings();
            settings.Settings = new IAnalyzerSettings[array.Length];

            for (var i = 0; i < array.Length; i++)
            {
                var jObject = array[i].Value<JObject>();
                var property = jObject.Property("#type");
                var type = property.Value.Value<string>();
                property.Remove();

                settings.Settings[i] = (IAnalyzerSettings)jObject.ToObject(Type.GetType(type));
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
                    var title = new Label
                    {
                        text = "Custom UI Elements"
                    };
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    var properties = new VisualElement
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    var tf = new TextField
                    {
                        value = "Test"
                    };
                    tf.AddToClassList("property-value");
                    properties.Add(tf);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] {"Unity", "Linting", "Lint"})
            };

            return provider;
        }

    }
}
