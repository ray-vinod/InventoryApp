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
    public partial class ReceiveIndex : ComponentBase, IDisposable
    {
        public List<ReceiveViewModel> receivevm;
        public List<PageUrl> pageUrlList;
        public bool spinnerOnOff = true;
        public IJSObjectReference jsModule;
        private bool isLock = false;

        [Inject]
        private AlertService AlertService { get; set; }
        [Inject]
        private UpdateService<UpdateModel> UpdateService { get; set; }
        [Inject]
        private ReceiveService ReceiveService { get; set; }
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        private ILogger<ReceiveIndex> Logger { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }




        protected override async Task OnInitializedAsync()
        {
            receivevm = new List<ReceiveViewModel>();

            pageUrlList = new List<PageUrl>
            {
                new PageUrl("receive/detail/", "Open", "oi-external-link", "btn-outline-info ml-2")
            };

            UpdateService.OnUpdateRequested += PageUpdateHandler;

            await LoadData(PagingParameter.CurrentPage, null);
        }

        private async void PageUpdateHandler(string property, UpdateModel model)
        {
            await InvokeAsync(async () =>
            {
                //On Record update
                if (property == null && model != null)
                {
                    if (model.Prefix != null)
                        NavigationManager.NavigateTo("/receive/index", true);

                    if (model.Suffix != null)
                        NavigationManager.NavigateTo("/receive/index", true);

                    if (model.Receive != null && property == null)
                    {
                        int index = receivevm.FindIndex(x => x.Id == model.Receive.Id);
                        receivevm.RemoveAt(index);
                        receivevm.Insert(index, model.Receive);
                    }

                    //hard refresh
                    if (model.PurchaseReturn != null)
                        NavigationManager.NavigateTo("/receive/index",true);
                }

                //On Record Create
                if (property != null)
                {
                    foreach (var load in property.Split(new char[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (load == "create")
                            await LoadData(PagingParameter.CurrentPage, null);

                        //hard refresh
                        if (load == "cancel")
                            NavigationManager.NavigateTo("/receive/index",true);
                    }
                }

                StateHasChanged();
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

            PagingParameter.TotalPages = ReceiveService.PageCount();
            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
                if (searchText != null)
                {
                    PagingParameter.TotalPages = 1;
                    AlertService.AddMessage(new Alert("Record Notfound!", AlertType.Error), 2);
                }
            }

            if (receivevm.Count == 0 && PagingParameter.CurrentPage != 1)
            {
                PagingParameter.CurrentPage -= 1;
                if (!isLock)
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
            receivevm.Clear();

            if (searchText != null)
            {
                await foreach (var item in ReceiveService.StreamListAsync(
                    page,
                    PagingParameter.PageSize,
                    filter: x => x.Product.Name.Contains(searchText) &&
                                 x.IsDelete != true,
                    orderBy: o => o.OrderByDescending(x => x.ReceiveDate)
                                    .ThenBy(x => x.Product.Prefix.Name)
                                    .ThenBy(x => x.Product.Name),
                    includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    receivevm.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
            else
            {
                await foreach (var item in ReceiveService.StreamListAsync(
                                    page,
                                    PagingParameter.PageSize,
                                    filter: x => x.IsDelete != true,
                                    orderBy: o => o.OrderByDescending(x => x.ReceiveDate)
                                                    .ThenBy(x => x.Product.Prefix.Name)
                                                    .ThenBy(x => x.Product.Name),
                                    includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    receivevm.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }

            Logger.LogInformation("Receive list loaded!");
            isLock = false;
        }

        private async Task Download()
        {
            jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/site.js");
            var receiveList = await ReceiveService.GetReportData();
            var fileContent = await ExcelReporter<ReceiveReportViewModel>.GetReports(receiveList,
                "Purchase List");

            await jsModule.InvokeVoidAsync("saveAsFile", "Inventory Report.xlsx",
                Convert.ToBase64String(fileContent));

            Logger.LogInformation("Records download complete");
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}