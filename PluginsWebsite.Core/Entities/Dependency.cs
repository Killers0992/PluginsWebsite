using PluginsWebsite.Core.Enums;

namespace PluginsWebsite.Core.Entities
{
    public class Dependency
    {
        public int Id { get; set; }
        public DependencyType Type { get; set; } = DependencyType.Unknown;

        public int FileId { get; set; }
        public LoaderFile File { get; set; }

        public string AssemblyName { get; set; }
        public string AssemblyVersion { get; set; }

        public int? LoaderId { get; set; }
        public Loader Loader { get; set; }

        public int? PluginId { get; set; }
        public Plugin Plugin { get; set; }

        public int? NugetId { get; set; }
        public Nuget Nuget { get; set; }
    }
}
