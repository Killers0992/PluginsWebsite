using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Requests
{
    public class SearchRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public int[] Categories { get; set; }
        public int[] Loaders { get; set; }
    }
}
