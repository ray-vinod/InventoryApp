using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class ProductCreate : IDisposable
    {

        public IEnumerable<Prefix> prefixes;
        public IEnumerable<Suffix> suffixes;
        public List<string> products;
        public Product product;
        public ElementReference firstInput;
        public bool getFocus = false;
        public string title = "Product-create";

        [Parameter] public Guid Id { get; set; }

        [Inject] private AlertService AlertService { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] public PrefixService PrefixService { get; set; }
        [Inject] public SuffixService SuffixService { get; set; }
        [Inject] public ProductService ProductService { get; set; }
        [Inject] public StockService StockService { get; set; }
        [Inject] public ILogger<ProductCreate> Logger { get; set; }
        [Inject] public UpdateService UpdateService { get; set; }



        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(0);
            product = new Product();
            products = new List<string>();
            prefixes = new List<Prefix>();
            suffixes = new List<Suffix>();

            UpdateService.OnUpdateRequested += PageUpdateHandler;

            await LoadPrefix();
            await LoadSuffix();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender || getFocus)
            {
                await firstInput.FocusAsync();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Id != Guid.Empty)
            {
                await EditEntity();
            }
        }

        public async void PageUpdateHandler(string property, bool isUpdate)
        {
            await InvokeAsync(async () =>
            {
                foreach (var item in property.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    if (item.Equals("prefix/index") && !isUpdate)
                    {
                        await LoadPrefix();
                    }

                    if (item.Equals("suffix/index") && !isUpdate)
                    {
                        await LoadSuffix();
                    }
                }

                StateHasChanged();
            });
        }

        private async Task AddEdit()
        {
            if (product.Name == null)
            {
                getFocus = true;
                return;
            }

            product.Name = product.Name.ToUpper();
            bool found = false;
            var isExist = await ProductService.GetItemsAsync(x => x.Name == product.Name);

            //Prevent duplication
            //The same medicine are can entry for deffirent form like tablet,syrup or injection
            if (isExist.Any())
            {
                foreach (var item in isExist)
                {
                    if (product.PrefixId == item.PrefixId && product.SuffixId == item.SuffixId &&
                        product.Id != item.Id)
                    {
                        found = true;
                    }

                    if (found)
                    {
                        Logger.LogInformation("{0} {1}", product.Name, AlertMessage.ExistInfo);
                        AlertService.AddMessage(new Alert(product.Name + AlertMessage.ExistInfo,
                            AlertType.Warning));

                        found = false;
                        getFocus = true;
                        return;
                    }
                }
            }

            //Adding product
            if (product.Id == Guid.Empty)
            {
                var isInserted = await ProductService.CreateAsync(product);

                if (isInserted != null)
                {
                    Logger.LogInformation("{0} {1}", product.Name, AlertMessage.AddInfo);

                    AlertService.AddMessage(new Alert(product.Name + AlertMessage.AddInfo,
                        AlertType.Success));

                    UpdateService.UpdatePage("product/index",false);

                    getFocus = true;
                    product = new Product();
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (product == isExist)
                {
                    AlertService.AddMessage(new Alert("No Change detected!", AlertType.Warning));
                }
                else
                {
                    if (product.PrefixId == Guid.Empty)
                    {
                        product.PrefixId = null;
                    }

                    if (product.SuffixId == Guid.Empty)
                    {
                        product.SuffixId = null;
                    }

                    //Update existing Product
                    var isUpdated = await ProductService.UpdateAsync(product);
                    if (isUpdated != null)
                    {
                        Logger.LogInformation("{0} {1}", product.Name, AlertMessage.UpdateInfo);
                        AlertService.AddMessage(new Alert(product.Name + AlertMessage.UpdateInfo,
                            AlertType.Success));

                        UpdateService.UpdatePage("product/index",true);
                        NavigationManager.NavigateTo("/product/index");
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private async Task EditEntity()
        {
            product = await ProductService.GetByIdAsync(Id);
            NavigationManager.NavigateTo("/product/edit", false);
            title = "Product-edit";
        }

        private async Task LoadSuffix()
        {
            suffixes = await SuffixService.GetItemsAsync();

            if (suffixes.Any())
            {
                suffixes = suffixes.OrderBy(x => x.Name);
            }
        }

        private async Task LoadPrefix()
        {
            prefixes = await PrefixService.GetItemsAsync();

            if (prefixes.Any())
            {
                prefixes = prefixes.OrderBy(x => x.Name);
            }
        }

        //Prefix Search function
        public async Task<IEnumerable<Prefix>> SearchAsync(string searchText)
        {
            return await Task.FromResult(prefixes.Where(p => p.Name.Contains(searchText.ToUpper())));
        }

        public async Task<IEnumerable<Suffix>> SearchSuffixAsync(string searchText)
        {
            return await Task.FromResult(suffixes.Where(p => p.Name.Contains(searchText.ToUpper())));
        }

        public void Reset()
        {
            title = "Product-create";
            product = new Product();
            NavigationManager.NavigateTo("/product/create", false);
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }

    }
}