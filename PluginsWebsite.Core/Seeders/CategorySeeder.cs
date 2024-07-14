using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Seeders
{
    public class CategorySeeder
    {
        public class CategoryInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
        }

        public static CategoryInfo[] Categories { get; } = new CategoryInfo[]
        {
            new CategoryInfo()
            {
                Id = 1,
                Icon = Icons.Material.TwoTone.Casino,
                Name = "Custom Gamemode",
            },
            new CategoryInfo()
            {
                Id = 2,
                Icon = Icons.Material.TwoTone.Map,
                Name = "Map Editing",
            },
            new CategoryInfo()
            {
                Id = 3,
                Icon = Icons.Material.TwoTone.Shield,
                Name = "Moderation",
            },
            new CategoryInfo()
            {
                Id = 4,
                Icon = Icons.Material.TwoTone.Camera,
                Name = "Misc",
            },
            new CategoryInfo()
            {
                Id = 5,
                Icon = Icons.Material.TwoTone.Biotech,
                Name = "New Mechanics",
            },
            new CategoryInfo()
            {
                Id = 6,
                Icon = Icons.Material.TwoTone.LibraryAdd,
                Name = "Library",
            },
            new CategoryInfo()
            {
                Id = 7,
                Icon = Icons.Material.TwoTone.Contacts,
                Name = "Admin Tools",
            },
            new CategoryInfo()
            {
                Id = 8,
                Icon = Icons.Material.TwoTone.DataObject,
                Name = "Dev Tools",
            },
            new CategoryInfo()
            {
                Id = 9,
                Icon = Icons.Material.TwoTone.Construction,
                Name = "ServerHost Tools",
            },
        };

    }
}
