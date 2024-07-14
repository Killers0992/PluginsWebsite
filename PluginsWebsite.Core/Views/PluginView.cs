using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Views
{
    public class PluginView
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string ImageURL { get; set; }
        public string AuthorName { get; set; }
        public bool IsOwner { get; set; }

        public List<string> SupportedLoaders { get; set; } = new List<string>();

        public IEnumerable<int> Categories { get; set; } = new List<int>();
        public IEnumerable<int> Loaders { get; set; } = new List<int>();

        public DateTime UpdatedOn { get; set; }

        public List<PluginFileView> Files { get; set; } = new List<PluginFileView>();
    }
}
