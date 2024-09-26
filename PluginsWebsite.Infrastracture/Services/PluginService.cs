using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mono.Cecil;
using Octokit;
using PluginsWebsite.Core.Entities;
using PluginsWebsite.Core.Enums;
using PluginsWebsite.Core.Interfaces;
using PluginsWebsite.Core.Responses;
using PluginsWebsite.Core.Views;
using PluginsWebsite.Infrastracture.Data;
using Author = PluginsWebsite.Core.Entities.Author;
using User = PluginsWebsite.Core.Entities.User;

namespace PluginsWebsite.Infrastracture.Services
{
    public class PluginService : IPluginService
    {
        private IHttpContextAccessor _httpContextAccessor;
        private IDbContextFactory<WebsiteContext> _context;
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private HttpClient _client;

        public PluginService(IDbContextFactory<WebsiteContext> context, IHttpContextAccessor accessor, UserManager<User> userManager, SignInManager<User> signInManager, HttpClient client)
        {
            _context = context;
            _httpContextAccessor = accessor;
            _userManager = userManager;
            _signInManager = signInManager;
            _client = client;
        }

        public class RepoFileInfo
        {
            public string LoaderId { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public int Size { get; set; }
            public string Hash { get; set; }
            public DateTime UpdatedAt { get; set; }
            public Dictionary<string, RefInfo> References { get; set; } = new Dictionary<string, RefInfo>();
        }

        public class RefInfo
        {
            public string Version { get; set; }
        }

        public async Task<PublishResponse> PublishAsync(string repositoryOwner, string repositoryName)
        {
            var user = _httpContextAccessor.HttpContext.User;

            if (user == null)
            {
                return new PublishResponse("Not logged in");
            }

            var loggedUser = await _userManager.GetUserAsync(user);

            if (loggedUser == null)
            {
                return new PublishResponse("Not logged in");
            }

            var token = await _userManager.GetAuthenticationTokenAsync(loggedUser, "GitHub", "access_token");

            GitHubClient client = new GitHubClient(new ProductHeaderValue("Killers0992"));
            client.Credentials = new Credentials(token);

            var current = await client.User.Current();

            Repository repo = null;
            try
            {
                repo = await client.Repository.Get(current.Login, repositoryName);
            }
            catch (Exception)
            {
                return new PublishResponse($"Repository {repositoryName} don't exists");
            }

            WebsiteContext context = await _context.CreateDbContextAsync();

            Author author = await context.Authors.FirstOrDefaultAsync(x => x.UserId == loggedUser.Id);

            if (author == null)
            {
                author = new Author()
                {
                    Type = AuthorType.User,
                    UserId = loggedUser.Id,
                };
                await context.Authors.AddAsync(author);
                await context.SaveChangesAsync();
            }

            Plugin plugin = await context.Plugins.FirstOrDefaultAsync(x => x.Name == repositoryName && (x.Author.Type == AuthorType.User ? x.Author.User.UserName : x.Author.Group.Name) == current.Login);

            if (plugin != null)
            {
                await context.DisposeAsync();
                return new PublishResponse("Plugin is already published!");
            }

            string desc = "Description not set!";

            using(var response = await _client.GetAsync($"https://raw.githubusercontent.com/{repo.Name}/{repo.Owner.Login}/{repo.DefaultBranch}/README.md"))
            {
                if (response.IsSuccessStatusCode)
                {
                    desc = await response.Content.ReadAsStringAsync();
                }
            }

            plugin = new Plugin()
            {
                Name = repo.Name,
                Description = desc,
                AuthorId = author.Id,
                UpdatedOn = repo.UpdatedAt.Date,
                CreatedOn = repo.CreatedAt.Date,
            };

            await context.Plugins.AddAsync(plugin);
            await context.SaveChangesAsync();

            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(repo.Id);

            List<Task> tasks = new List<Task>();

            foreach (var release in releases)
            {
                tasks.Add(ProcessRelease(plugin.Id, _context, release));
            }

            await Task.WhenAll(tasks);

            await context.DisposeAsync();
            return new PublishResponse(plugin.Id);
        }

        public async Task ProcessRelease(int id, IDbContextFactory<WebsiteContext> con, Release r)
        {
            var context = await con.CreateDbContextAsync();

            List<Task<RepoFileInfo>> tasks = new List<Task<RepoFileInfo>>();

            foreach(var asset in r.Assets)
            {
                tasks.Add(ProcessAsset(asset));
            }

            var results = await Task.WhenAll(tasks);

            var files = results.Where(x => x != null).ToArray();

            if (files.Length == 0)
            {
                await context.DisposeAsync();
                return;
            }

            PluginFile pFile = new PluginFile()
            {
                PluginId = id,
                ReleaseId = r.Id,
                Version = r.TagName,
                Changelog = r.Body,
                PublishedAt = r.PublishedAt.HasValue ? r.PublishedAt.Value.ToLocalTime().Date : DateTime.MinValue,
                CreatedAt = r.CreatedAt.ToLocalTime().Date,
            };

            await context.Files.AddAsync(pFile);
            await context.SaveChangesAsync();

            Dictionary<string, List<RepoFileInfo>> filesSeperatedByLoader = new Dictionary<string, List<RepoFileInfo>>();

            foreach (var file in files)
            {
                if (filesSeperatedByLoader.ContainsKey(file.LoaderId))
                    filesSeperatedByLoader[file.LoaderId].Add(file);
                else
                    filesSeperatedByLoader.Add(file.LoaderId, new List<RepoFileInfo>() { file });
            }

            foreach (var loaderFile in filesSeperatedByLoader)
            {
                var loader = await context.Loaders.FirstOrDefaultAsync(x => x.Name == loaderFile.Key);

                if (loader == null) continue;

                foreach (var file in loaderFile.Value)
                {
                    LoaderFile lFile = new LoaderFile()
                    {
                        AssemblyName = file.Name,
                        Version = r.TagName,
                        Url = file.Url,
                        FileId = pFile.Id,
                        LoaderId = loader.Id,
                        Size = file.Size,
                        UpdatedAt = file.UpdatedAt,
                        Hash = file.Hash,
                    };

                    await context.LoaderFiles.AddAsync(lFile);
                    await context.SaveChangesAsync();

                    foreach (var dependency in file.References)
                    {
                        var findDep = await context.LoaderFiles.Where(x => x.AssemblyName == dependency.Key && x.Version == dependency.Value.Version).Select(x => x.File.Plugin).FirstOrDefaultAsync();

                        DependencyType type = DependencyType.Unknown;
                        int? nugetId = null;
                        int? pluginId = null;

                        if (findDep == null)
                        {
                            string nugetName = dependency.Key.ToLower();

                            var nuget = await context.Nugets.FirstOrDefaultAsync(x => x.Name == nugetName && x.Version == dependency.Value.Version);

                            if (nuget == null)
                            {
                                using (var request = await _client.GetAsync($"https://api.nuget.org/v3-flatcontainer/{nugetName}/{dependency.Value.Version}/{nugetName}.nuspec"))
                                {
                                    if (request.IsSuccessStatusCode)
                                    {
                                        nuget = new NuGet()
                                        {
                                            Name = nugetName,
                                            Version = dependency.Value.Version,
                                        };

                                        await context.Nugets.AddAsync(nuget);
                                        await context.SaveChangesAsync();

                                        nugetId = nuget.Id;
                                        type = DependencyType.Nuget;
                                    }
                                }
                            }
                            else
                            {
                                nugetId = nuget.Id;
                                type = DependencyType.Nuget;
                            }
                        }
                        else
                        {
                            pluginId = findDep.Id;
                            type = DependencyType.Plugin;
                        }

                        Dependency dep = new Dependency()
                        {
                            Type = type,
                            AssemblyName = dependency.Key,
                            AssemblyVersion = dependency.Value.Version,
                            FileId = lFile.Id,
                            NugetId = nugetId,
                            PluginId = pluginId,
                        };

                        await context.Dependencies.AddAsync(dep);
                        await context.SaveChangesAsync();
                    }
                }
            }

            await context.DisposeAsync();
        }
        

        public static string GetMD5checksum(byte[] inputData)
        {
            //convert byte array to stream
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            stream.Write(inputData, 0, inputData.Length);

            //important: get back to start of stream
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            //get a string value for the MD5 hash.
            using (var md5Instance = System.Security.Cryptography.MD5.Create())
            {
                var hashResult = md5Instance.ComputeHash(stream);

                //***I did some formatting here, you may not want to remove the dashes, or use lower case depending on your application
                return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
            }
        }

        public async Task<RepoFileInfo> ProcessAsset(ReleaseAsset asset)
        {
            // Ignore all non binary files
            if (asset.ContentType != "application/octet-stream" && asset.ContentType != "application/x-msdownload")
                return null;

            string extension = Path.GetExtension(asset.Name);

            if (extension != ".dll")
                return null;

            byte[] bytes = await _client.GetByteArrayAsync(asset.BrowserDownloadUrl);

            using (var ms = new MemoryStream(bytes))
            {
                AssemblyDefinition def = AssemblyDefinition.ReadAssembly(ms);

                bool assemblySupportsNetFramework = false;

                foreach (var attr in def.CustomAttributes)
                {
                    if (attr.AttributeType.FullName != "System.Runtime.Versioning.TargetFrameworkAttribute") continue;
                    string frameworkVersion = attr.Properties[0].Argument.Value.ToString();
                    if (frameworkVersion.StartsWith(".NET Framework 4.8"))
                    {
                        assemblySupportsNetFramework = true;
                        break;
                    }
                }

                if (assemblySupportsNetFramework)
                {
                    Dictionary<string, RefInfo> refs = new Dictionary<string, RefInfo>();

                    foreach (var assemblyRef in def.MainModule.AssemblyReferences)
                    {
                        if (refs.ContainsKey(assemblyRef.Name)) continue;

                        refs.Add(assemblyRef.Name, new RefInfo() { Version = assemblyRef.Version.ToString(3) });
                    }

                    return new RepoFileInfo()
                    {
                        LoaderId = refs.Any(x => x.Key.Contains("Exiled")) ? "exiled" : "nwapi",
                        Name = def.Name.Name,
                        Url = asset.BrowserDownloadUrl,
                        Size = asset.Size,
                        UpdatedAt = asset.UpdatedAt.Date,
                        References = refs,
                        Hash = GetMD5checksum(bytes),
                    };
                }
            }

            return null;
        }

        public async Task<PluginView> GetPluginAsync(int pluginId)
        {
            var context = await _context.CreateDbContextAsync();

            var user = _httpContextAccessor.HttpContext.User;

            User loggedInUser  = null;

            if (user != null)
            {
                loggedInUser = await _userManager.GetUserAsync(user);
            }

            PluginView item = await context.Plugins
                .Where(x => x.Id == pluginId)
                .Select(x => new PluginView()
                {
                    Name = x.Name,
                    Description = x.Description,
                    AuthorName = x.Author.Type == AuthorType.User ? x.Author.User.UserName : x.Author.Group.Name,
                    ImageURL = x.ImageUrl,
                    ShortDescription = x.ShortDescription,
                    Categories = x.Categories.Select(x => x.CategoryId).ToList(),
                    Loaders = x.Files.SelectMany(z => z.LoaderFiles.Select(b => b.LoaderId)).Distinct().ToArray(),
                    IsOwner = loggedInUser != null && x.Author.Type == AuthorType.User ? x.Author.UserId == loggedInUser.Id : false,
                    UpdatedOn = x.UpdatedOn,
                }).FirstOrDefaultAsync();

            if (item == null)
            {
                await context.DisposeAsync();
                return null;
            }

            item.Files = await context.Files.Where(x => x.PluginId == pluginId).Select(y => new PluginFileView()
            {
                Id = y.Id,
                Changelog = y.Changelog,
                GameVersion = y.GameVersionId.HasValue ? y.GameVersion.Name : "unknown",
                Version = y.Version,
                PublishedAt = y.PublishedAt,
                CreatedAt = y.CreatedAt,
            }).ToListAsync();

            foreach(var file in item.Files)
            {
                var files = await context.LoaderFiles.Where(x => x.FileId == file.Id).Include(x => x.Loader).ToListAsync();

                foreach(var file2 in files)
                {
                    if (!item.SupportedLoaders.Contains(file2.Loader.Name))
                        item.SupportedLoaders.Add(file2.Loader.Name);

                    var itemFile = new LoaderFileView()
                    {
                        Name = file2.AssemblyName,
                        Version = file2.Version,
                        GameVersion = file.GameVersion,
                        Url = file2.Url,
                        Size = file2.Size,
                        UpdatedAt = file2.UpdatedAt,
                        ChangeLog = file.Changelog,
                    };

                    foreach(var dependency in await context.Dependencies.Where(x => x.FileId == file2.Id).ToListAsync())
                    {
                        if (itemFile.Dependencies.ContainsKey(dependency.AssemblyName)) continue;

                        itemFile.Dependencies.Add(dependency.AssemblyName, new DependencyView()
                        {
                            Type = dependency.Type,
                            AssemblyVersion = dependency.AssemblyVersion,
                        });
                    }

                    if (file.LoaderFiles.ContainsKey(file2.Loader.Name))
                        file.LoaderFiles[file2.Loader.Name].Add(itemFile);
                    else
                        file.LoaderFiles.Add(file2.Loader.Name, new List<LoaderFileView>() { itemFile });
                }
            }

            await context.DisposeAsync();
            return item;
        }

        public async Task<SearchResponse> SearchPluginsAsync(int page, int pageSize, string search = null, int[] categories = null, int[] loaders = null)
        {
            var context = await _context.CreateDbContextAsync();

            var itemsReq = context.Plugins.AsQueryable();
            var totalReq = context.Plugins.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                itemsReq = itemsReq.Where(x => x.Name == search || x.Name.Contains(search));
                totalReq = itemsReq.Where(x => x.Name == search || x.Name.Contains(search));
            }

            if (categories != null && categories.Length != 0)
            {
                itemsReq = itemsReq.Where(x => x.Categories.Any(x => categories.Contains(x.CategoryId)));
                totalReq = totalReq.Where(x => x.Categories.Any(x => categories.Contains(x.CategoryId)));
            }

            if (loaders != null && loaders.Length != 0)
            {
                itemsReq = itemsReq.Where(x => x.Files.Any(y => y.LoaderFiles.Any(z => loaders.Contains(z.LoaderId))));
                totalReq = totalReq.Where(x => x.Files.Any(y => y.LoaderFiles.Any(z => loaders.Contains(z.LoaderId))));
            }

            SearchResponse resp = new SearchResponse();

            resp.TotalItems = await totalReq.CountAsync();

            resp.Items = await itemsReq
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(x => new PluginItemView()
                {
                    Id = x.Id,
                    Name = x.Name,
                    AuthorName = x.Author.Type == AuthorType.User ? x.Author.User.UserName : x.Author.Group.Name,
                    ShortDescription = x.ShortDescription,
                    ImageURL = x.ImageUrl,
                    Categories = x.Categories.Select(x => x.CategoryId).ToArray(),
                    Loaders = x.Files.SelectMany(z => z.LoaderFiles.Select(b => b.LoaderId)).Distinct().ToArray(),
                    UpdatedOn = x.UpdatedOn,
                }).ToArrayAsync();

            await context.DisposeAsync();

            return resp;
        }

        public async Task<PluginItemView[]> GetPluginsAsync()
        {
            var context = await _context.CreateDbContextAsync();

            PluginItemView[] items = await context.Plugins
                .Select(X => new PluginItemView()
                {
                    Id = X.Id,
                    Name = X.Name,
                    AuthorName = X.Author.Type == AuthorType.User ? X.Author.User.UserName : X.Author.Group.Name,
                    ShortDescription = X.ShortDescription,
                    ImageURL = X.ImageUrl,
                    UpdatedOn = X.UpdatedOn,
                }).ToArrayAsync();

            await context.DisposeAsync();
            return items;
        }

        public async Task<bool> DeletePluginAsync(int pluginId)
        {
            var user = _httpContextAccessor.HttpContext.User;

            if (user == null)
            {
                return false;
            }

            var loggedUser = await _userManager.GetUserAsync(user);

            if (loggedUser == null)
            {
                return false;
            }

            var context = await _context.CreateDbContextAsync();

            var plugin = await context.Plugins.Where(x => x.Id == pluginId && x.Author.UserId == loggedUser.Id).FirstOrDefaultAsync();

            if (plugin == null)
            {
                return false;
            }

            context.Plugins.Remove(plugin);
            await context.SaveChangesAsync();

            await context.DisposeAsync();
            return true;
        }

        public async Task<bool> UpdatePluginAsync(int pluginId, string desc, string shortDesc, string imageUrl, int[] categories)
        {
            var user = _httpContextAccessor.HttpContext.User;

            if (user == null)
            {
                return false;
            }

            var loggedUser = await _userManager.GetUserAsync(user);

            if (loggedUser == null)
            {
                return false;
            }

            var context = await _context.CreateDbContextAsync();

            var plugin = await context.Plugins.Where(x => x.Id == pluginId && x.Author.UserId == loggedUser.Id).FirstOrDefaultAsync();

            if (plugin == null)
            {
                return false;
            }

            plugin.ShortDescription = shortDesc;
            plugin.Description = desc;
            plugin.ImageUrl = imageUrl;

            context.Plugins.Update(plugin);
            await context.SaveChangesAsync();

            int[] currentCategories = await context.CategoryLinks.Where(x => x.PluginId == pluginId).Select(x => x.CategoryId).ToArrayAsync();

            var toRemove = categories.Except(currentCategories);

            var toAdd = currentCategories.Except(categories);

            foreach(var remove in await context.CategoryLinks.Where(x => x.PluginId == pluginId && toRemove.Contains(x.CategoryId)).ToListAsync())
            {
                context.CategoryLinks.Remove(remove);
            }

            foreach (var add in toAdd)
            {
                PluginCategoryLink catLink = new PluginCategoryLink()
                {
                    CategoryId = add,
                    PluginId = pluginId,
                };

                await context.CategoryLinks.AddAsync(catLink);
            }

            await context.SaveChangesAsync();

            await context.DisposeAsync();

            return true;
        }
    }
}
