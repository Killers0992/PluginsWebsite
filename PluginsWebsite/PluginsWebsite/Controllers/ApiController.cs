using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PluginsWebsite.Core.Interfaces;
using PluginsWebsite.Core.Requests;

namespace PluginsWebsite.Controllers
{
    public class ApiController : Controller
    {
        private readonly IPluginService _pluginService;

        public ApiController(IPluginService pluginService)
        {
            _pluginService = pluginService;
        }

        [HttpPost("~/api/search")]
        public async Task<IActionResult> SearchAsync([FromBody] SearchRequest request)
        {
            return Ok(await _pluginService.SearchPluginsAsync(request.Page, request.PageSize, request.Search, request.Categories, request.Loaders));
        }

        [HttpGet("~/api/plugins")]
        public async Task<IActionResult> GetPluginsAsync()
        {
            return Ok(await _pluginService.GetPluginsAsync());
        }

        [HttpGet("~/api/plugin")]
        public async Task<IActionResult> GetPluginAsync(int pluginId)
        {
            return Ok(await _pluginService.GetPluginAsync(pluginId));
        }

        [Authorize]
        [HttpPost("~/api/publish")]
        public async Task<IActionResult> PublishAsync( [FromBody] PublishRequest request)
        {
            return Ok(await _pluginService.PublishAsync(request.RepositoryOwner, request.RepositoryName));
        }

        [Authorize]
        [HttpGet("~/api/plugin/delete")]
        public async Task<IActionResult> DeletePluginAsync(int pluginId)
        {
            return Ok(await _pluginService.DeletePluginAsync(pluginId));
        }

        [Authorize]
        [HttpPost("~/api/plugin/update")]
        public async Task<IActionResult> UpdatePluginAsync([FromBody] UpdatePluginRequest request)
        {
            return Ok(await _pluginService.UpdatePluginAsync(request.PluginId, request.Description, request.ShortDescription, request.ImageUrl, request.Categories));
        }
    }
}
