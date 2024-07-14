using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PluginsWebsite.Client.Forms;
using PluginsWebsite.Core.Interfaces;

namespace PluginsWebsite.Client.Pages
{
    [Authorize]
    public partial class Publish : ComponentBase
    {
        private PublishForm model = new PublishForm();
        string error;
        bool processing;

        [Inject]
        public IPluginService PluginService { get; set; }

        [Inject]
        public NavigationManager Navigator { get; set; }

        private async Task OnValidSubmit(EditContext context)
        {
            if (processing) return;

            processing = true;
            StateHasChanged();
            var response = await PluginService.PublishAsync(model.Owner, model.RepositoryName);

            if (!response.IsSuccess)
            {
                processing = false;
                error = response.Message;
                StateHasChanged();
                return;
            }

            Navigator.NavigateTo($"/plugin/{response.PluginId}");
        }
    }
}
