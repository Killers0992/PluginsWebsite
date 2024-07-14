using Microsoft.VisualStudio.Services.Common.Internal;
using MudBlazor;
using Newtonsoft.Json;
using PluginsWebsite.Core.Entities;
using PluginsWebsite.Core.Interfaces;
using PluginsWebsite.Core.Requests;
using PluginsWebsite.Core.Responses;
using PluginsWebsite.Core.Views;
using System.Drawing.Printing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PluginsWebsite.Client.Services
{
    public class ClientPluginService : IPluginService
    {
        private readonly HttpClient _client;

        public ClientPluginService(HttpClient client)
        {
            _client = client;
        }

        public async Task<PublishResponse> PublishAsync(string repositoryOwner, string repositoryName)
        {
            return await _client.PostAsJsonAsync<PublishResponse>($"api/publish", new PublishRequest(repositoryOwner, repositoryName));
        }

        public async Task<PluginView> GetPluginAsync(int pluginId)
        {
            return await _client.GetFromJsonAsync<PluginView>($"api/plugin?pluginId={pluginId}");
        }

        public async Task<SearchResponse> SearchPluginsAsync(int page, int pageSize, string search = null, int[] categories = null, int[] loaders = null)
        {
            return await _client.PostAsJsonAsync<SearchResponse>($"api/search", new SearchRequest()
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                Categories = categories,
                Loaders = loaders,
            });
        }


        public async Task<PluginItemView[]> GetPluginsAsync()
        {
            return await _client.GetFromJsonAsync<PluginItemView[]>("api/plugins") ?? [];
        }

        public async Task<bool> DeletePluginAsync(int pluginId)
        {
            return await _client.GetFromJsonAsync<bool>($"api/plugin/delete?pluginId={pluginId}");
        }

        public async Task<bool> UpdatePluginAsync(int pluginId, string desc, string shortDesc, string imageUrl, int[] categories)
        {
            return await _client.PostAsJsonAsync($"api/plugin/update", new UpdatePluginRequest()
            {
                PluginId = pluginId,
                Description = desc,
                ShortDescription = shortDesc,
                ImageUrl = imageUrl,
                Categories = categories,
            });
        }
    }
}
