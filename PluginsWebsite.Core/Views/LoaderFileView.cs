using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Views
{
    public class LoaderFileView
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string GameVersion { get; set; }
        public string ChangeLog { get; set; }
        public string Url { get; set; }
        public int Size { get; set; }
        public DateTime UpdatedAt { get; set; }

        [JsonIgnore]
        public bool ShowDetails;

        public Dictionary<string, DependencyView> Dependencies { get; set; } = new Dictionary<string, DependencyView>();
    }
}
