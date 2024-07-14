using System.ComponentModel.DataAnnotations;

namespace PluginsWebsite.Core.Entities
{
    public class Nuget
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
