﻿using InventoryApp.Helpers;
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
        private ILogger<StockIndex> Logger { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public UpdateService<UpdateModel> UpdateService { get; set; }



        protected override async Task OnInitializedAsync()
        {
            stocks = new List<Stock>();
            pageUrlList = new List<PageUrl>();

            UpdateService.OnUpdateRequested += PageUpdateHandler;

            await LoadData(PagingParameter.CurrentPage, null);
        }

        private async void PageUpdateHandler(string property, UpdateModel model)
        {
            await InvokeAsync(async () =>
            {
                if (model != null)
                {
                    if (model.Prefix != null)
                        NavigationManager.NavigateTo("/receive/index", true);

                    if (model.Suffix != null)
                        NavigationManager.NavigateTo("/receive/index", true);

                    if (model.SaleReturn != null)
                    {
                        var stock = stocks.Find(x => x.Id == model.SaleReturn.ProductId);
                        if (stock != null)
                        {
                            stock.TotalIssueReturn += model.SaleReturn.Quantity;
                            stock.InStock += model.SaleReturn.Quantity;

                            UpdateStock(model.SaleReturn.ProductId, stock);
                        }
                        else
                            await LoadData(PagingParameter.CurrentPage, null);
                    }

                    if (model.Issue != null && property== "create/issue")
                    {
                        var stock = stocks.Find(x => x.Id == model.Issue.ProductId);
                        if (stock != null)
                        {
                            stock.TotalIssue += model.Issue.Quantity;
                            stock.InStock -= model.Issue.Quantity;

                            UpdateStock(model.Issue.ProductId, stock);
                        }
                        else
                            await LoadData(PagingParameter.CurrentPage, null);
                    }

                    if (model.Receive != null && property.Equals("create"))
                    {
                        var stock = stocks.Find(x => x.Id == model.Receive.ProductId);
                        if (stock != null)
                        {
                            stock.TotalReceive += model.Receive.Quantity;
                            stock.InStock += model.Receive.Quantity;

                            UpdateStock(model.Receive.ProductId, stock);
                        }
                        else
                            await LoadData(PagingParameter.CurrentPage, null);
                    }

                    if (model.PurchaseReturn != null)
                    {
                        var stock = stocks.Find(x => x.Id == model.PurchaseReturn.ProductId);
                        if (stock != null)
                        {
                            stock.TotalReceiveReturn += model.PurchaseReturn.Quantity;
                            stock.InStock -= model.PurchaseReturn.Quantity;

                            UpdateStock(model.PurchaseReturn.ProductId, stock);
                        }
                        else
                            await LoadData(PagingParameter.CurrentPage, null);
                    }

                    //Purchase Entry Cancel
                    if (model.Receive != null && property.Equals("cancel"))
                    {
                        var stock = stocks.Find(x => x.Id == model.Receive.ProductId);
                        if (stock != null)
                        {
                            stock.TotalReceive -= model.Receive.Quantity;
                            stock.InStock -= model.Receive.Quantity;

                            UpdateStock(model.Receive.ProductId, stock);
                        }
                        else
                            await LoadData(PagingParameter.CurrentPage, null);
                    }

                    //Sale entry cancel
                    if (model.Issue !=null && property.Equals("cancel"))
                    {
                        var stock = stocks.Find(x=>x.Id == model.Issue.ProductId);

                        if (stock != null)
                        {
                            stock.TotalIssue -= model.Issue.Quantity;
                            stock.InStock += model.Issue.Quantity;

                            UpdateStock(model.Issue.ProductId, stock);
                        }
                        else
                            await LoadData(PagingParameter.CurrentPage,null);
                    }
                }

                StateHasChanged();
            });
        }

        private void UpdateStock(Guid id, Stock stock)
        {
            int index = stocks.FindIndex(x => x.Id == id);

            stocks.RemoveAt(index);
            stocks.Insert(index, stock);
        }

        private async Task LoadData(int page, string searchText)
        {
            if (!isLock)
            {
                while (isLock)
                {
                    Logger.LogInformation("System is busy ...");
                    await Task.Delay(100);
                }
                await CallData(page, searchText);
            }

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
            await Task.Delay(100);

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

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}