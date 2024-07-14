using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Views
{
    public class PluginItemView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string ImageURL { get; set; }

        public int[] Categories { get; set; }
        public int[] Loaders { get; set; }

        public string AuthorName { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
