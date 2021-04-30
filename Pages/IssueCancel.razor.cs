using Blazored.Modal;
using Blazored.Modal.Services;
using InventoryApp.Components;
using InventoryApp.Helpers;
using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Models.ViewModels;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class IssueCancel : IDisposable
    {
        public List<IssueViewModel> issues;
        public List<PageUrl> pageUrlList;
        public bool spinnerOnOff = true;
        public string title = "Issue-cancel";

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
        private StockService StockService { get; set; }
        [Inject]
        private ReceiveService ReceiveService { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; set; }



        protected override async Task OnInitializedAsync()
        {
            issues = new List<IssueViewModel>();
            pageUrlList = new List<PageUrl>
            {
                new PageUrl("issue/abort/", "Abort", "oi-trash", "btn-outline-danger ml-2"),
                new PageUrl(null, "Approve", "oi-chek", "btn-outline-info ml-2")
            };

            UpdateService.OnUpdateRequested += PageUpdateHandler;

            await LoadData(PagingParameter.CurrentPage);
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Id != Guid.Empty)
            {
                await Abort(Id);
            }
        }

        private async void PageUpdateHandler(string property, UpdateModel model)
        {
            await InvokeAsync(async () =>
            {
                await LoadData(PagingParameter.CurrentPage);
                StateHasChanged();
            });
        }

        private async Task LoadData(int page)
        {
            await CallData(page);

            PagingParameter.TotalPages = IssueService.PageCount();
            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
            }

            if (issues.Count == 0 && PagingParameter.CurrentPage != 1)
            {
                PagingParameter.CurrentPage -= 1;
                await CallData(PagingParameter.CurrentPage);
            }
        }

        //EventCallback for paging navigation linkmo
        public async Task SelectedPage(int page)
        {
            PagingParameter.CurrentPage = page;
            await LoadData(PagingParameter.CurrentPage);
        }

        //Finally Delete Item
        private async Task Delete(Guid id)
        {
            var issue = await IssueService.GetByIdAsync(id);
            var stock = await StockService.GetByIdAsync(issue.ProductId);
            var receive = await ReceiveService.GetByIdAsync(issue.PurchaseId);

            var parameters = new ModalParameters();
            parameters.Add(nameof(DeleteConfirmModal.Message),
                $"{issue.Product.Prefix?.Name} {issue.Product.Name}");

            var options = new ModalOptions
            {
                HideCloseButton = true,
                DisableBackgroundCancel = true,
            };

            var fromMessage = Modal.Show<DeleteConfirmModal>("Do you want to delete this?",
                parameters, options);

            var result = await fromMessage.Result;
            if (!result.Cancelled)
            {
                if (issue.ReturnQty == 0)
                {
                    stock.TotalIssue -= issue.Quantity;
                    stock.InStock = (stock.TotalReceive - stock.TotalReceiveReturn) -
                        (stock.TotalIssue - stock.TotalIssueReturn);
                    issue.IsDelete = true;

                    //Manage Receive table on issued item has deleted
                    if (receive.IsUse == true)
                    {
                        receive.IsUse = false;
                        receive.UseQuantity = receive.Quantity - issue.Quantity;
                    }
                    else
                    {
                        receive.UseQuantity -= issue.Quantity;
                    }
                }
                else
                {
                    //manage stock
                    int qty = issue.Quantity - issue.ReturnQty;
                    stock.TotalIssue -= qty;
                    stock.InStock += qty;

                    //Create new Issue Item for new Deletetion
                    var newIssue = new Issue
                    {
                        IsDelete = true,
                        Quantity = qty,
                        ReturnQty = 0,
                        Note = issue.Note,
                        Remarks = issue.Remarks,
                        IssueBy = issue.IssueBy,
                        PurchaseId = issue.PurchaseId,
                        IssueDate = issue.IssueDate,
                        ProductId = issue.ProductId,
                    };

                    await IssueService.CreateAsync(newIssue);

                    //Manage Receive table on issued item has deleted
                    if (receive.IsUse == true)
                    {
                        receive.IsUse = false;
                        receive.UseQuantity = receive.Quantity - newIssue.Quantity;
                    }
                    else
                    {
                        receive.UseQuantity -= newIssue.Quantity;
                    }

                    //Manage Issue
                    issue.Quantity = issue.ReturnQty;
                    issue.ReturnQty = 0;
                    issue.Note = null;
                    issue.IsUse = true;
                }

                await IssueService.UpdateAsync(issue);
                await ReceiveService.UpdateAsync(receive);
                var updatedStock = await StockService.UpdateAsync(stock);

                if (updatedStock != null)
                {
                    await LoadData(PagingParameter.CurrentPage);
                    UpdateService.UpdatePage();
                    AlertService.AddMessage(new Alert($"{updatedStock.Product.Name} removed successfully!",
                        AlertType.Info));
                }
            }
        }

        private async Task CallData(int page)
        {
            issues.Clear();
            await foreach (var issue in IssueService.StreamListAsync(
                page,
                PagingParameter.PageSize,
                orderBy: o => o.OrderByDescending(x => x.IssueDate)
                    .ThenBy(x => x.Product.Name),
                includeProperties: "Product,Product.Prefix,Product.Suffix"))
            {
                var vm = new IssueViewModel
                {
                    Id = issue.Id,
                    IssueDate = issue.IssueDate.ToShortDateString(),
                    Name = $"{issue.Product.Prefix?.Name} {issue.Product.Name} {issue.Product.Suffix?.Name}",
                    Qty = issue.Quantity,
                    IssueBy = issue.IssueBy,
                    Note = issue.Note,
                };

                issues.Add(vm);
                spinnerOnOff = false;
                StateHasChanged();
            }
        }

        private async Task Abort(Guid id)
        {
            title = "Issue-abort";
            NavigationManager.NavigateTo("/issue/abort", false);

            var item = await IssueService.GetByIdAsync(id);
            item.Note = null;
            await IssueService.UpdateAsync(item);

            AlertService.AddMessage(new Alert("Task completed successfully!", AlertType.Info));
            await LoadData(PagingParameter.CurrentPage);
        }
        
        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}