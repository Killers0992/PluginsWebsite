namespace PluginsWebsite.Core.Entities
{
    public class Plugin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; } = "/images/scpsl.png";
        public string ShortDescription { get; set; } = "Short description not set!";
        public string Description { get; set; } = "Description not set!";

        public int AuthorId { get; set; }
        public Author Author { get; set; }

        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public virtual ICollection<PluginCategoryLink> Categories { get; set; }
        public virtual ICollection<PluginFile> Files { get; set; }
    }
}
