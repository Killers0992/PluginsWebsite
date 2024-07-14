using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
