using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PluginsWebsite.Core.Entities;

namespace PluginsWebsite.Infrastracture.Data
{
    public class WebsiteContext : IdentityDbContext<User, Role, string>
    {
        public WebsiteContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Loader> Loaders { get; set; }
        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PluginCategoryLink> CategoryLinks { get; set; }
        public DbSet<PluginFile> Files { get; set; }
        public DbSet<LoaderFile> LoaderFiles { get; set; }
        public DbSet<Dependency> Dependencies { get; set; }
        public DbSet<GameVersion> GameVersions { get; set; }
        public DbSet<Nuget> Nugets { get; set; }
    }
}
