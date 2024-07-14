namespace PluginsWebsite.Core.Entities
{
    public class PluginCategoryLink
    {
        public int Id { get; set; }

        public int PluginId { get; set; }
        public Plugin Plugin { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
