using PluginsWebsite.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Responses
{
    public class SearchResponse
    {
        public PluginItemView[] Items { get; set; }
        public int TotalItems { get; set; }
    }
}
