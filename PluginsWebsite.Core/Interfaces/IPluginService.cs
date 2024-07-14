using PluginsWebsite.Core.Entities;
using PluginsWebsite.Core.Responses;
using PluginsWebsite.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsWebsite.Core.Interfaces
{
    public interface IPluginService
    {
        Task<PublishResponse> PublishAsync(string repositoryOwner, string repositoryName);
        Task<SearchResponse> SearchPluginsAsync(int page, int pageSize, string search = null, int[] categories = null, int[] loaders = null);
        Task<PluginItemView[]> GetPluginsAsync();
        Task<PluginView> GetPluginAsync(int pluginId);
        Task<bool> DeletePluginAsync(int pluginId);
        Task<bool> UpdatePluginAsync(int pluginId, string desc, string shortDesc, string imageUrl, int[] categories);
    }
}
