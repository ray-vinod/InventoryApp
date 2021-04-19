using InventoryApp.Helpers;
using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Models.ViewModels;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class StockIndex : IDisposable
    {
        public List<Stock> stocks;
        public List<PageUrl> pageUrlList;
        public bool spinnerOnOff = true;
        public IJSObjectReference jsModule;
        private bool isLock = false;

        [Inject]
        AlertService AlertService { get; set; }
        [Inject]
        private StockService StockService { get; set; }
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
        private ILogger<StockIndex> Logger { get; set; }
        [Inject]
        public UpdateService<Product> ProductUpdateService { get; set; }
        [Inject]
        public UpdateService<Receive> ReceiveUpdateService { get; set; }
        [Inject]
        public UpdateService<Issue> IssueUpdateService { get; set; }



        protected override async Task OnInitializedAsync()
        {
            stocks = new List<Stock>();
            pageUrlList = new List<PageUrl>();

            ProductUpdateService.OnUpdateRequested += ProductStockUpdateHandler;
            ReceiveUpdateService.OnUpdateRequested += ReceiveStockUpdateHandler;
            IssueUpdateService.OnUpdateRequested += IssueStockUpdateHandler;

            await LoadData(PagingParameter.CurrentPage, null);
        }

        private async void ProductStockUpdateHandler(Product product)
        {
            await InvokeAsync(async () =>
            {
                if (isLock)
                    await LoadData(PagingParameter.CurrentPage, null);

                StateHasChanged();
            });
        }

        private async void ReceiveStockUpdateHandler(Receive receive)
        {
            await InvokeAsync(async () =>
            {
                if (isLock)
                    await LoadData(PagingParameter.CurrentPage, null);

                StateHasChanged();
            });
        }

        private async void IssueStockUpdateHandler(Issue issue)
        {
            await InvokeAsync(async () =>
            {
                if (isLock)
                    await LoadData(PagingParameter.CurrentPage, null);

                StateHasChanged();
            });
        }

        private async Task LoadData(int page, string searchText)
        {
            await CallData(page, searchText);
            PagingParameter.TotalPages = StockService.PageCount();
            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
                if (searchText != null)
                {
                    PagingParameter.TotalPages = 1;
                    Logger.LogWarning("Record Not found!");
                    AlertService.AddMessage(new Alert("Record Notfound!", AlertType.Error), 2);
                }
            }

            if (stocks.Count == 0 && PagingParameter.CurrentPage != 1)
            {
                PagingParameter.CurrentPage -= 1;
                await CallData(PagingParameter.CurrentPage, null);
            }
        }

        private async Task CallData(int page, string searchText)
        {
            isLock = true;
            stocks.Clear();
            if (searchText != null)
            {
                await foreach (var item in StockService.StreamListAsync(
                page,
                PagingParameter.PageSize,
                filter: x => x.Product.Name.Contains(searchText),
                orderBy: o => o.OrderBy(x => x.Product.Name),
                includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    stocks.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
            else
            {
                await foreach (var item in StockService.StreamListAsync(
                page,
                PagingParameter.PageSize,
                orderBy: o => o.OrderBy(x => x.Product.Name),
                includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    stocks.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
            isLock = false;
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

        private async Task Download()
        {
            jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/site.js");
            var stockList = await StockService.GetStockReport();
            var fileContent = await ExcelReporter<StockViewModel>.GetReports(stockList, "Stock List");
            await jsModule.InvokeVoidAsync("saveAsFile", "Inventory Report.xlsx", Convert.ToBase64String(fileContent));
        }

        private async Task Detail(Guid id)
        {
            await Task.Delay(0);
        }

        public void Dispose()
        {
            ProductUpdateService.OnUpdateRequested -= ProductStockUpdateHandler;
            ReceiveUpdateService.OnUpdateRequested += ReceiveStockUpdateHandler;
            IssueUpdateService.OnUpdateRequested += IssueStockUpdateHandler;
        }
    }
}