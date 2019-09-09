using System.Collections.Generic;

namespace Editor
{
    public struct PackageManifest
    {
        public IDictionary<string, string> Dependencies { get; set; }
        public ScopedRegistry[] ScopedRegistries { get; set; }
    }

    public struct ScopedRegistry
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string[] Scopes { get; set; }
    }
}
