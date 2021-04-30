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

        protected override async Task OnParametersSetAsync()
        {
            if (Id != Guid.Empty)
            {
                NavigationManager.NavigateTo("/receive/index", false);
                await Cancel(Id);
            }
        }

        private async void PageUpdateHandler(string property, UpdateModel model)
        {
            await InvokeAsync(async () =>
            {
                await LoadData(PagingParameter.CurrentPage, null);
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
        }

        private async Task Download()
        {
            jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/site.js");
            var issueList = await IssueService.GetReport();
            var fileContent = await ExcelReporter<IssueReportViewModel>.GetReports(issueList, "Issue List");
            await jsModule.InvokeVoidAsync("saveAsFile", "Inventory Report.xlsx", Convert.ToBase64String(fileContent));
        }

        //Request for remove from list which is aproved by the authorize person
        private async Task Cancel(Guid id)
        {
            //var issue = await IssueService.GetByIdAsync(id);

            //adding filter and order then convert in single result
            var issuResult = await IssueService.GetItemsAsync(
                filter: x => x.Id == id,
                orderBy: o => o.OrderBy(x => x.Product.Prefix.Name)
                                .ThenBy(x => x.Product.Name)
                                .ThenByDescending(x => x.IssueDate),
                includeProperties: "Product,Product.Prefix,Product.Suffix");

            var issue = issuResult.FirstOrDefault();

            var parameters = new ModalParameters();
            parameters.Add(nameof(DeleteConfirmModal.Message), $"{issue.Product.Prefix?.Name} {issue.Product.Name} "
                                                    + $" {issue.Product.Suffix?.Name}");
            parameters.Add(nameof(DeleteConfirmModal.ShowInput), true);

            var options = new ModalOptions { HideCloseButton = true, DisableBackgroundCancel = true, };
            var fromMessage = Modal.Show<DeleteConfirmModal>("Do you want to remove this item?", parameters, options);

            var result = await fromMessage.Result;
            if (!result.Cancelled)
            {
                issue.Note = result.Data.ToString().ToUpper();
                await IssueService.UpdateAsync(issue);

                UpdateService.UpdatePage();
                Logger.LogInformation("{} is requested for cancellation!", issue.Product.Name);
                AlertService.AddMessage(new Alert("Request for cancel entry has been sent!",
                    AlertType.Info));
            }
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}