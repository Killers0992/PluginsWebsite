using PluginsWebsite.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Views
{
    public class DependencyView
    {
        public DependencyType Type { get; set; }
        public string AssemblyVersion { get; set; }
    }
}
