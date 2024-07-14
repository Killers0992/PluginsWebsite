using PluginsWebsite.Core.Enums;

namespace PluginsWebsite.Core.Entities
{
    public class Author
    {
        public int Id { get; set; }
        public AuthorType Type { get; set; }
        public string? UserId { get; set; }
        public User User { get; set; }
        public int? GroupId { get; set; }
        public Group Group { get; set; }
    }
}
