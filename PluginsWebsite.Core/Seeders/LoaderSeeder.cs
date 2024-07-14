using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Seeders
{
    public class LoaderSeeder
    {
        public class LoaderInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string GroupOwner { get; set; }
            public string Icon { get; set; }
        }

        public static LoaderInfo[] Loaders { get; } = new LoaderInfo[]
        {
            new LoaderInfo()
            {
                Id = 1,
                Icon = "nwapi",
                Name = "NW Plugin API",
                GroupOwner = "Northwood",
            },
            new LoaderInfo()
            {
                Id = 2,
                Icon = "exiled",
                Name = "Exiled",
                GroupOwner = "ExiledTeam"
            }
        };

    }
}
