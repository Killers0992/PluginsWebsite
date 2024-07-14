namespace PluginsWebsite.Core.Entities
{
    public class LoaderFile
    {
        public int Id { get; set; }

        public string Version { get; set; }
        public string AssemblyName { get; set; }
        public string Url { get; set; }
        public int Size { get; set; }
        public string Hash { get; set; }

        public int LoaderId { get; set; }
        public Loader Loader { get; set; }

        public int FileId { get; set; }
        public PluginFile File { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
