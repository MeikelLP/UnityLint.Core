using System;
using UnityEngine;

namespace Editor.Analyzers.Asset.RecommendedRules
{
    public class AssetAnalyzerSettings : IAnalyzerSettings
    {
        public int MaxFileSize = 1024 * 1024 * 2;

        public PrefabPaths PrefabPaths = new PrefabPaths();
        public ConventionSettings Conventions = new ConventionSettings();
    }

    public class PrefabPaths
    {
        public string Scripts = "Assets/Scripts";
        public string Prefabs = "Assets/Prefabs";
        public string Audio = "Assets/Audio";
        public string Images = "Assets/Images";
        public string Models = "Assets/Models";
        public string Scenes = "Assets/Scenes";
    }

    public class ConventionSettings
    {
        public NamingConvention Models = NamingConvention.UpperCamelCase;
        public NamingConvention Scripts = NamingConvention.UpperCamelCase;
        public NamingConvention Audio = NamingConvention.UpperCamelCase;
        public NamingConvention Prefabs = NamingConvention.UpperCamelCase;
        public NamingConvention Images = NamingConvention.UpperCamelCase;
        public NamingConvention Scenes = NamingConvention.UpperCamelCase;
    }

    public enum NamingConvention
    {
        LowerCamelCase,
        UpperCamelCase,
        AllCaps,
        SnakeCase
    }
}
