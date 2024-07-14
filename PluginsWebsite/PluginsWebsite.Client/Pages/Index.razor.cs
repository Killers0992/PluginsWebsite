using Microsoft.AspNetCore.Components;
using MudBlazor;
using PluginsWebsite.Client.Services;
using PluginsWebsite.Core.Entities;
using PluginsWebsite.Core.Interfaces;
using PluginsWebsite.Core.Seeders;
using PluginsWebsite.Core.Views;

namespace PluginsWebsite.Client.Pages
{
    public partial class Index : ComponentBase
    {
        private string searchString = string.Empty;
        private HashSet<CategoryItem> _selectedCategories { get; set; }
        private HashSet<LoaderItem> _selectedLoaders { get; set; }
        public class CategoryItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public int Amount { get; set; }
        }

        public class LoaderItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public int Amount { get; set; }
        }

        private HashSet<CategoryItem> SelectedCategories
        {
            get => _selectedCategories;
            set
            {
                _selectedCategories = value;
                table.ReloadServerData();
            }
        }

        private HashSet<LoaderItem> SelectedLoaders
        {
            get => _selectedLoaders;
            set
            {
                _selectedLoaders = value;
                table.ReloadServerData();
            }
        }


        private HashSet<CategoryItem> Categories;
        private HashSet<LoaderItem> Loaders;


        private MudTable<PluginItemView> table;

        protected override void OnInitialized()
        {
            Categories = CategorySeeder.Categories.Select(x => new CategoryItem()
            {
                Id = x.Id,
                Name = x.Name,
                Amount = 0,
                Icon = x.Icon,
            }).ToHashSet();

            Loaders = LoaderSeeder.Loaders.Select(x => new LoaderItem()
            {
                Id = x.Id,
                Name = x.Name,
                Amount = 0,
                Icon = x.Icon,
            }).ToHashSet();

            base.OnInitialized();
        }

        [Inject]
        public IPluginService PluginService { get; set; }

        private async Task<TableData<PluginItemView>> ServerReload(TableState state)
        {
            var searchResponse = await PluginService.SearchPluginsAsync(state.Page, state.PageSize, searchString, SelectedCategories.Select(x => x.Id).ToArray(), SelectedLoaders.Select(x => x.Id).ToArray());

            return new TableData<PluginItemView>() { TotalItems = searchResponse.TotalItems, Items = searchResponse.Items };
        }

        private void OnSearch(string text)
        {
            searchString = text;
            table.ReloadServerData();
        }
    }
}
