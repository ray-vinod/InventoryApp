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
        public List<Prefix> prefixes;
        public List<Suffix> suffixes;
        public List<string> products;
        public Product product;
        public ElementReference firstInput;
        public bool getFocus = false;
        public string title = "Product-create";
        private bool isLock = false;

        [Parameter] public Guid Id { get; set; }

        [Inject] private AlertService AlertService { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] public PrefixService PrefixService { get; set; }
        [Inject] public SuffixService SuffixService { get; set; }
        [Inject] public ProductService ProductService { get; set; }
        [Inject] public StockService StockService { get; set; }
        [Inject] public ILogger<ProductCreate> Logger { get; set; }
        [Inject] public UpdateService<Product> UpdateService { get; set; }
        [Inject] public UpdateService<Prefix> UpdateServicePrefix { get; set; }
        [Inject] public UpdateService<Suffix> UpdateServiceSuffix { get; set; }



        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(0);
            product = new Product();
            products = new List<string>();
            prefixes = new List<Prefix>();
            suffixes = new List<Suffix>();

            UpdateServicePrefix.OnUpdateRequested += PrefixUpdateHandler;
            UpdateServiceSuffix.OnUpdateRequested += SuffixUpdateHandler;

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

        public async void PrefixUpdateHandler(Prefix prefix)
        {
            await InvokeAsync(async () =>
            {
                if (prefix != null)
                {
                    //find index of list and remove
                    int index = prefixes.FindIndex(x => x.Id == prefix.Id);
                    prefixes.RemoveAt(index);

                    //find the updated entity and add to list at index
                    prefixes.Insert(index, prefix);
                }
                else
                {
                    if (!isLock)
                    {
                        while (isLock)
                        {
                            Logger.LogInformation("System is busy ...");
                            await Task.Delay(100);
                        }

                        await LoadPrefix();
                    }
                }

                StateHasChanged();
            });
        }

        public async void SuffixUpdateHandler(Suffix suffix)
        {
            await InvokeAsync(async () =>
            {
                if (suffix != null)
                {
                    //find index of list and remove
                    int index = suffixes.FindIndex(x => x.Id == suffix.Id);
                    suffixes.RemoveAt(index);

                    //find the updated entity and add to list at index
                    suffixes.Insert(index, suffix);
                }
                else
                {
                    if (!isLock)
                    {
                        while (isLock)
                        {
                            Logger.LogInformation("System is busy ...");
                            await Task.Delay(100);
                        }

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

            //create product
            if (product.Id == Guid.Empty)
            {
                var isInserted = await ProductService.CreateAsync(product);

                if (isInserted != null)
                {
                    Logger.LogInformation("{0} {1}", product.Name, AlertMessage.AddInfo);

                    AlertService.AddMessage(new Alert(product.Name + AlertMessage.AddInfo,
                        AlertType.Success));

                    UpdateService.UpdatePage();

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

                        UpdateService.UpdatePage(isUpdated);
                        NavigationManager.NavigateTo("/product/index", false);
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
            isLock = true;
            suffixes = (List<Suffix>)await SuffixService.GetItemsAsync();

            if (suffixes.Any())
            {
                suffixes.OrderBy(x => x.Name);
            }

            isLock = false;
        }

        private async Task LoadPrefix()
        {
            isLock = true;
            prefixes = (List<Prefix>)await PrefixService.GetItemsAsync();

            if (prefixes.Any())
            {
                prefixes.OrderBy(x => x.Name);
            }

            isLock = false;
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
            UpdateServicePrefix.OnUpdateRequested -= PrefixUpdateHandler;
            UpdateServiceSuffix.OnUpdateRequested -= SuffixUpdateHandler;
        }

    }
}