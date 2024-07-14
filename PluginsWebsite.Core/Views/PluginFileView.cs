using PluginsWebsite.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Views
{
    public class PluginFileView
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public string Changelog { get; set; }
        public string GameVersion { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public Dictionary<string, List<LoaderFileView>> LoaderFiles { get; set; } = new Dictionary<string, List<LoaderFileView>>();
    }
}
