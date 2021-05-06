using InventoryApp.Helpers;
using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Models.ViewModels;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class IssueReturn : IDisposable
    {
        public List<SaleReturnViewModel> saleReturnsvm;
        public bool spinnerOnOff = true;
        public IJSObjectReference jsModule;
        public bool isLock = false;

        [Inject]
        public UpdateService<UpdateModel> UpdateService { get; set; }
        [Inject]
        public SaleReturnService SaleReturnService { get; set; }
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        public AlertService AlertService { get; set; }



        protected override async Task OnInitializedAsync()
        {
            saleReturnsvm = new List<SaleReturnViewModel>();

            UpdateService.OnUpdateRequested += PageUpdateHandler;

            await LoadData(PagingParameter.CurrentPage, null);
        }

        private async void PageUpdateHandler(string property, UpdateModel model)
        {
            await InvokeAsync(async () =>
            {
                if (model.SaleReturn != null)
                {
                    if (!isLock)
                    {
                        while (isLock)
                        {
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

            PagingParameter.TotalPages = SaleReturnService.PageCount();
            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
                if (searchText != null)
                {
                    PagingParameter.TotalPages = 1;
                    AlertService.AddMessage(new Alert("Record Notfound!", AlertType.Error), 2);
                }
            }

            if (saleReturnsvm.Count == 0 && PagingParameter.CurrentPage != 1)
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
            saleReturnsvm.Clear();

            if (searchText != null)
            {
                await foreach (var item in SaleReturnService.StreamListAsync(
                    page,
                    PagingParameter.PageSize,
                    filter: x => x.Product.Name.Contains(searchText),
                    orderBy: o => o.OrderByDescending(x => x.ReturnDate),
                    includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    var vm = new SaleReturnViewModel
                    {
                        Return_Date = item.ReturnDate.ToShortDateString(),
                        Name = $"{item.Product.Prefix?.Name} {item.Product.Name} {item.Product.Suffix?.Name}",
                        Qty = item.Quantity,
                        Retrun_By = item.ReturneBy,
                        Remarks = item.Remarks,
                    };

                    saleReturnsvm.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
            else
            {
                await foreach (var item in SaleReturnService.StreamListAsync(
                    page,
                    PagingParameter.PageSize,
                    orderBy: o => o.OrderByDescending(x => x.ReturnDate),
                    includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    var vm = new SaleReturnViewModel
                    {
                        Return_Date = item.ReturnDate.ToShortDateString(),
                        Name = $"{item.Product.Prefix?.Name} {item.Product.Name} " +
                               $"{item.Product.Suffix?.Name}",
                        Qty = item.Quantity,
                        Retrun_By = item.ReturneBy,
                        Remarks = item.Remarks,
                    };

                    saleReturnsvm.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }

            isLock = false;
        }

        private async Task Download()
        {
            jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/site.js");
            var prList = await SaleReturnService.GetReport();
            var fileContent = await ExcelReporter<SaleReturnViewModel>.GetReports(prList, "Sales Returns");
            await jsModule.InvokeVoidAsync("saveAsFile", "Inventory Report.xlsx", Convert.ToBase64String(fileContent));
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}