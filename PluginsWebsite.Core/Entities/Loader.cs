namespace PluginsWebsite.Core.Entities
{
    public class Loader
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }

        public int AuthorId { get; set; }
        public Author Author { get; set; }

        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
