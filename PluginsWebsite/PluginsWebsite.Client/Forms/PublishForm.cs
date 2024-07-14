using System.ComponentModel.DataAnnotations;

namespace PluginsWebsite.Client.Forms
{
    public class PublishForm
    {
        public string Owner { get; set; }

        [Required]
        public string RepositoryName { get; set; }
        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You need to accept rules of publishing policy!")]
        public bool AcceptedRules { get; set; }
    }
}
