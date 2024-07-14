using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Requests
{
    public class UpdatePluginRequest
    {
        public int PluginId { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public int[] Categories { get; set; }
    }
}
