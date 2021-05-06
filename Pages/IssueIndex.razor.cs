using Blazored.Modal.Services;
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
    public partial class IssueIndex : IDisposable
    {
        public List<IssueViewModel> issuevm;
        public bool spinnerOnOff = true;
        public List<PageUrl> pageUrlList;
        public IJSObjectReference jsModule;
        public string title = "Issue-index";
        private bool isLock = false;

        [Parameter]
        public Guid Id { get; set; }
        [CascadingParameter]
        IModalService Modal { get; set; }

        [Inject]
        private AlertService AlertService { get; set; }
        [Inject]
        private UpdateService<UpdateModel> UpdateService { get; set; }
        [Inject]
        private IssueService IssueService { get; set; }
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
        private ILogger<IssueIndex> Logger { get; set; }



        protected override async Task OnInitializedAsync()
        {
            issuevm = new List<IssueViewModel>();
            pageUrlList = new List<PageUrl>
            {
                new PageUrl("issue/detail/", "Open", "oi-external-link", "btn-outline-info ml-2")
            };

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
                        NavigationManager.NavigateTo("/issue/index", true);

                    if (model.Suffix != null)
                        NavigationManager.NavigateTo("/issue/index", true);

                    if (model.Issue != null)
                        await LoadData(PagingParameter.CurrentPage, null);
                }
                else
                {
                    if (property != null)
                    {
                        foreach (var load in property.Split(new char[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!isLock)
                            {
                                while (isLock)
                                {
                                    Logger.LogInformation("System is busy ...");
                                    await Task.Delay(100);
                                }

                                if (load == "issue/index")
                                    await LoadData(PagingParameter.CurrentPage, null);
                            }
                        }
                    }
                }

                StateHasChanged();
            });
        }

        private async Task LoadData(int page, string searchText)
        {
            await CallData(page, searchText);

            PagingParameter.TotalPages = IssueService.PageCount();
            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
                if (searchText != null)
                {
                    PagingParameter.TotalPages = 1;
                    AlertService.AddMessage(new Alert("Record Notfound!", AlertType.Error), 2);
                }
            }

            if (issuevm.Count == 0 && PagingParameter.CurrentPage != 1)
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
            issuevm.Clear();

            if (searchText != null)
            {
                await foreach (var item in IssueService.StreamListAsync(
               page,
               PagingParameter.PageSize,
               filter: x => x.Product.Name.Contains(searchText) &&
                            x.IsDelete != true,
               orderBy: o => o.OrderBy(x => x.Product.Prefix.Name)
                                    .ThenBy(x => x.Product.Prefix.Name)
                                    .ThenByDescending(x => x.IssueDate),
               includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    issuevm.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }
            else
            {
                await foreach (var item in IssueService.StreamListAsync(
                    page,
                    PagingParameter.PageSize,
                    filter: x => x.IsDelete != true,
                    orderBy: o => o.OrderBy(x => x.Product.Prefix.Name)
                                    .ThenBy(x => x.Product.Prefix.Name)
                                    .ThenByDescending(x => x.IssueDate),
                    includeProperties: "Product,Product.Prefix,Product.Suffix"))
                {
                    issuevm.Add(item);
                    spinnerOnOff = false;
                    StateHasChanged();
                }
            }

            isLock = false;
        }

        private async Task Download()
        {
            jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/site.js");
            var issueList = await IssueService.GetReport();
            var fileContent = await ExcelReporter<IssueReportViewModel>.GetReports(issueList, "Issue List");
            await jsModule.InvokeVoidAsync("saveAsFile", "Inventory Report.xlsx", Convert.ToBase64String(fileContent));
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}