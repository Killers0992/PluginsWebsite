using Microsoft.AspNetCore.Components;
using MudBlazor;
using PluginsWebsite.Client.Dialogs;
using PluginsWebsite.Core.Interfaces;
using PluginsWebsite.Core.Seeders;
using PluginsWebsite.Core.Views;
using Toolbelt.Blazor.HeadElement;
using static PluginsWebsite.Core.Seeders.CategorySeeder;

namespace PluginsWebsite.Client.Pages
{
    public partial class ViewPlugin : ComponentBase
    {
        private PluginView Plugin = new PluginView();
        private MudTable<LoaderFileView> filesTable;
        private string _currentLoader;
        private string _retypePlugin;
        private IEnumerable<CategoryInfo> _selectedCategories = new List<CategoryInfo>();
        private CategoryInfo _selectedCategory;

        [Parameter]
        public int PluginID { get; set; }

        private LoaderFileView[] _content;

        [Inject]
        public IPluginService PluginService { get; set; }

        [Inject] protected IDialogService Dialog { get; set; }

        [Inject] protected ISnackbar Snackbar { get; set; }

        [Inject] protected NavigationManager Navigator { get; set; }

        [Inject] protected IHeadElementHelper HeadElement { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Plugin = await PluginService.GetPluginAsync(PluginID);
            _selectedCategories = CategorySeeder.Categories.Where(x => Plugin.Categories.Contains(x.Id));

            if (Plugin == null)
            {
                Snackbar.Add($"Plugin with id {PluginID} not exists!", Severity.Error);
                Navigator.NavigateTo("/");
                return;
            }

            var desc = string.Concat(
                    Plugin.ShortDescription,
                    Environment.NewLine,
                    Environment.NewLine,
                    $"Loaders: {(Plugin.SupportedLoaders.Count == 0 ? "none" : string.Join(", ", Plugin.SupportedLoaders))}",
                    Environment.NewLine,
                    $"Author: {Plugin.AuthorName}");

            await HeadElement.SetMetaElementsAsync(
                MetaElement.ByProp("description", desc),
                MetaElement.ByProp("og:title", Plugin.Name),
                MetaElement.ByProp("og:image", Plugin.ImageURL),
                MetaElement.ByProp("og:description", desc));

            if (Plugin.SupportedLoaders.Count > 0)
                _currentLoader = Plugin.SupportedLoaders[0];
        }

        private async Task<TableData<LoaderFileView>> ServerReload(TableState state)
        {
            if (_currentLoader == null)
                return new TableData<LoaderFileView>();

            _content = Plugin.Files.Where(x => x.LoaderFiles.ContainsKey(_currentLoader)).SelectMany(x => x.LoaderFiles.Values).SelectMany(x => x).ToArray();

            switch (state.SortLabel.ToLower())
            {
                case "filename":
                    _content = _content.OrderByDirection(state.SortDirection, x => x.Name).ToArray();
                    break;
                case "version":
                    _content = _content.OrderByDirection(state.SortDirection, x => x.Version).ToArray();
                    break;
                case "slversion":
                    _content = _content.OrderByDirection(state.SortDirection, x => x.GameVersion).ToArray();
                    break;
                case "uploaded":
                    _content = _content.OrderByDirection(state.SortDirection, x => x.UpdatedAt).ToArray();
                    break;
                case "size":
                    _content = _content.OrderByDirection(state.SortDirection, x => x.Size).ToArray();
                    break;
            }

            return new TableData<LoaderFileView>() { TotalItems = _content.Length, Items = _content };
        }

        public void OpenChangelogs(LoaderFileView file)
        {
            Dialog.Show<ChangelogsDialog>($"Changelogs for {file.Version}", new DialogParameters()
            {
                { "Content", file.ChangeLog }
            }, new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true });
        }

        public void OpenDependencies(LoaderFileView file)
        {
            Dialog.Show<DependenciesDialog>($"Dependencies", new DialogParameters()
            {
                { "Content", file.Dependencies }
            }, new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true });
        }

        public void RowClickEvent(TableRowClickEventArgs<LoaderFileView> rowClick)
        {
            foreach(var item in _content)
            {
                if (item == rowClick.Item)
                    item.ShowDetails = !item.ShowDetails;
                else
                    item.ShowDetails = false;
            }
            StateHasChanged();
        }

        public async Task DeletePlugin()
        {
            if (await PluginService.DeletePluginAsync(PluginID))
            {
                Navigator.NavigateTo("/");
            }
        }

        public async Task SavePlugin()
        {
            await PluginService.UpdatePluginAsync(PluginID, Plugin.Description, Plugin.ShortDescription, Plugin.ImageURL, Plugin.Categories.ToArray());
        }

        public string SelectionFunc(List<string> categories)
        {
            return string.Join(", ", _selectedCategories.Select(x => x.Name));
        }

        public async Task ActiveLoaderChanged(int loaderId)
        {
            _currentLoader = Plugin.SupportedLoaders[loaderId];
            filesTable.ReloadServerData();
        }
    }
}
