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
    public partial class ReceiveReturn
    {
        public List<PurchaseReturnViewModel> purchasevm;
        public List<PageUrl> pageUrlList;
        public bool spinnerOnOff = true;
        public IJSObjectReference jsModule;
        private bool isLock = false;

        [Inject]
        public UpdateService<UpdateModel> UpdateService { get; set; }
        [Inject]
        public PurchaseReturnService PurchaseReturnService { get; set; }
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }
        [Inject]
        public ILogger<ReceiveReturn> Logger { get; set; }



        protected override async Task OnInitializedAsync()
        {
            purchasevm = new List<PurchaseReturnViewModel>();
            pageUrlList = new List<PageUrl>
            {
                new PageUrl("receive/detail/", "Details", "oi-list", "btn-outline-info ml-2"),
            };

            UpdateService.OnUpdateRequested += PageUpdateHandler;

            await LoadData(PagingParameter.CurrentPage, null);
        }

        private async void PageUpdateHandler(string property, UpdateModel model)
        {
            await InvokeAsync(async () =>
            {
                if (model != null && model.PurchaseReturn != null)
                {
                    await LoadData(PagingParameter.CurrentPage, null);
                    StateHasChanged();
                }
            });
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

            PagingParameter.TotalPages = PurchaseReturnService.PageCount();
            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
                if (searchText != null)
                {
                    PagingParameter.TotalPages = 1;
                    AlertService.AddMessage(new Alert("Record Notfound!", AlertType.Error), 2);
                }
            }

            if (purchasevm.Count == 0 && PagingParameter.CurrentPage != 1)
            {
                PagingParameter.CurrentPage -= 1;
                await CallData(PagingParameter.CurrentPage, null);
            }
        }

        //EventCallback for paging navigation linkmo
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
            purchasevm.Clear();

            if (searchText != null)
            {
                await foreach (var item in PurchaseReturnService.StreamListAsync(
                page,
                PagingParameter.PageSize,
                filter: x => x.Product.Name.Contains(searchText),
                orderBy: o => o.OrderBy(x => x.ReturnDate)
                    .ThenBy(x => x.Product.Prefix.Name)
                    .ThenBy(x => x.Product.Name),
                includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    purchasevm.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
            else
            {
                await foreach (var item in PurchaseReturnService.StreamListAsync(
                page,
                PagingParameter.PageSize,
                orderBy: o => o.OrderBy(x => x.ReturnDate)
                    .ThenBy(x => x.Product.Prefix.Name)
                    .ThenBy(x => x.Product.Name),
                includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    purchasevm.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }

            isLock = false;
        }

        private async Task Download()
        {
            jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/site.js");
            var prList = await PurchaseReturnService.GetReport();
            var fileContent = await ExcelReporter<PurchaseReturnViewModel>.GetReports(prList, "Purchase Returns");
            await jsModule.InvokeVoidAsync("saveAsFile", "Inventory Report.xlsx", Convert.ToBase64String(fileContent));
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}