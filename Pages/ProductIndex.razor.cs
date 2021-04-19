using Blazored.Modal;
using Blazored.Modal.Services;
using InventoryApp.Components;
using InventoryApp.Helpers;
using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Models.ViewModels;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace InventoryApp.Pages
{
    public partial class ProductIndex : IDisposable
    {
        private List<ProductViewModel> products;
        List<PageUrl> urls;
        private bool spinnerOnOff = true;
        private bool isLock = false;

        [CascadingParameter]
        IModalService Modal { get; set; }

        [Inject]
        private AlertService AlertService { get; set; }
        [Inject]
        public PrefixService PrefixService { get; set; }
        [Inject]
        public SuffixService SuffixService { get; set; }
        [Inject]
        public ProductService ProductService { get; set; }
        [Inject]
        public StockService StockService { get; set; }
        [Inject]
        public ILogger<ProductIndex> Logger { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public UpdateService<Product> UpdateService { get; set; }
        [Inject]
        public UpdateService<Prefix> UpdateServicePrefix { get; set; }
        [Inject]
        public UpdateService<Suffix> UpdateServiceSuffix { get; set; }




        protected override async Task OnInitializedAsync()
        {
            products = new List<ProductViewModel>();
            urls = new List<PageUrl>
            {
                new PageUrl("product/edit/", "Edit", "oi-pencil", "btn-outline-info ml-2"),
                new PageUrl(null, "Delete", "oi-trash", "btn-outline-danger ml-2")
            };

            UpdateService.OnUpdateRequested += PageUpdateHandler;
            UpdateServicePrefix.OnUpdateRequested += PrefixUpdateHandler;
            UpdateServiceSuffix.OnUpdateRequested += SuffixUpdateHandler;

            await LoadData(PagingParameter.CurrentPage, null);

        }

        public async void SuffixUpdateHandler(Suffix suffix)
        {
            await InvokeAsync(async () =>
            {
                if (suffix != null)
                {
                    var listvm = new List<ProductViewModel>();

                    foreach (var product in products)
                    {
                        var item = await ProductService.GetItemsAsync(
                            filter: x => x.Id == product.Id,
                            includeProperties: "Prefix,Suffix");

                        if (item.First().SuffixId != null && item.First().SuffixId == suffix.Id)
                        {
                            var vm = new ProductViewModel
                            {
                                Id = product.Id,
                                Name = item.FirstOrDefault().Prefix?.Name + " " +
                                item.FirstOrDefault().Name + " " +
                                suffix.Name,
                                Group = $"{item.FirstOrDefault().GroupName}",
                            };

                            listvm.Add(vm);
                        }
                        else
                            listvm.Add(product);
                    }

                    products.Clear();
                    products.AddRange(listvm);
                }

                StateHasChanged();
            });
        }

        public async void PrefixUpdateHandler(Prefix prefix)
        {
            await InvokeAsync(async () =>
            {
                if (prefix != null)
                {
                    var listvm = new List<ProductViewModel>();

                    foreach (var product in products)
                    {
                        var item = await ProductService.GetItemsAsync(
                            filter: x => x.Id == product.Id,
                            includeProperties: "Prefix,Suffix");

                        if (item.First().PrefixId != null && item.First().PrefixId == prefix.Id)
                        {
                            var vm = new ProductViewModel
                            {
                                Id = product.Id,
                                Name = prefix.Name + " " +
                                item.FirstOrDefault().Name + " " +
                                item.FirstOrDefault().Suffix?.Name,
                                Group = $"{item.FirstOrDefault().GroupName}",
                            };

                            listvm.Add(vm);
                        }
                        else
                            listvm.Add(product);
                    }

                    products.Clear();
                    products.AddRange(listvm);
                }

                StateHasChanged();
            });
        }

        public async void PageUpdateHandler(Product product)
        {
            await InvokeAsync(async () =>
            {
                if (product != null)
                {
                    int index = products.FindIndex(x => x.Id == product.Id);
                    products.RemoveAt(index);

                    products.Insert(index, product);
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

                        await LoadData(PagingParameter.CurrentPage, null);
                    }
                }

                StateHasChanged();
            });
        }

        private async Task LoadData(int page, string searchText)
        {
            await CallData(page, searchText);

            PagingParameter.TotalPages = ProductService.PageCount();

            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
                if (searchText != null)
                {
                    PagingParameter.TotalPages = 1;
                    AlertService.AddMessage(new Alert("Record Notfound!", AlertType.Error), 2);
                }
            }

            if (products.Count == 0 && PagingParameter.CurrentPage != 1)
            {
                PagingParameter.CurrentPage -= 1;
                await CallData(PagingParameter.CurrentPage, null);
            }
        }

        //EventCallback for paging navigation link
        public async Task SelectedPage(int page)
        {
            PagingParameter.CurrentPage = page;
            await LoadData(PagingParameter.CurrentPage, null);
        }

        private async Task OnSearchTexChanged(ChangeEventArgs e)
        {
            string searchText = e.Value.ToString().ToUpper();
            await LoadData(PagingParameter.CurrentPage, searchText);
        }

        private async Task CallData(int page, string searchText)
        {
            isLock = true;
            products.Clear();

            if (searchText != null)
            {
                await foreach (var item in ProductService.StreamListAsync(page,
                        PagingParameter.PageSize,
                        filter: x => x.Name.Contains(searchText),
                        orderBy: o => o.OrderBy(x => x.Prefix.Name).ThenBy(x => x.Name),
                        includeProperties: "Prefix,Suffix"))
                {
                    products.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
            else
            {
                await foreach (var item in ProductService.StreamListAsync(page,
                       PagingParameter.PageSize,
                       orderBy: o => o.OrderBy(x => x.Prefix.Name).ThenBy(x => x.Name),
                       includeProperties: "Prefix,Suffix"))
                {
                    products.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }

            Logger.LogInformation("Loading records");
            isLock = false;
        }

        private async Task Delete(Guid id)
        {
            var stock = await StockService.GetByIdAsync(id);
            if (stock != null)
            {
                Logger.LogWarning("{0} is cann't delete", stock.Product.Name);
                AlertService.AddMessage(new Alert("You cann't delete this product", AlertType.Error));
                return;
            }

            var product = await ProductService.GetByIdAsync(id);
            string prefix = string.Empty;
            if (product.Prefix != null)
            {
                prefix = product.Prefix.Name;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(DeleteConfirmModal.Message), $"{prefix} {product.Name}");

            var options = new ModalOptions
            {
                HideCloseButton = true,
                DisableBackgroundCancel = true,
            };

            var fromMessage = Modal.Show<DeleteConfirmModal>("Do you want to delete this?", parameters, options);
            var result = await fromMessage.Result;
            if (!result.Cancelled)
            {
                bool isDelete = await ProductService.DeleteAsync(id);
                if (isDelete)
                {
                    Logger.LogInformation("{0} {1}", product.Name, AlertMessage.DeleteInfo);
                    AlertService.AddMessage(new Alert(product.Name + AlertMessage.DeleteInfo,
                        AlertType.Error));

                    UpdateService.UpdatePage();
                }
            }
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
            UpdateServicePrefix.OnUpdateRequested -= PrefixUpdateHandler;
            UpdateServiceSuffix.OnUpdateRequested -= SuffixUpdateHandler;
        }
    }
}