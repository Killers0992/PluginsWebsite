namespace PluginsWebsite.Core.Entities
{
    public class PluginFile
    {
        public int Id { get; set; }

        public int ReleaseId { get; set; }

        public string Version { get; set; }

        public string Changelog { get; set; }

        public int PluginId { get; set; }
        public Plugin Plugin { get; set; }

        public int? GameVersionId { get; set; }
        public GameVersion GameVersion { get; set; }

        public DateTime PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<LoaderFile> LoaderFiles { get; set; }
    }
}
