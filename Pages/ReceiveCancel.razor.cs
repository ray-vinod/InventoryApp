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
    public partial class ReceiveCancel : IDisposable
    {
        public List<ReceiveViewModel> receivevm;
        public List<PageUrl> pageUrlList;
        public bool spinnerOnOff = true;
        public string title = "Receive-cancel";
        public bool isLock = false;

        [CascadingParameter]
        IModalService Modal { get; set; }
        [Parameter]
        public Guid Id { get; set; }

        [Inject]
        private AlertService AlertService { get; set; }
        [Inject]
        private ReceiveService ReceiveService { get; set; }
        [Inject]
        private StockService StockService { get; set; }
        [Inject]
        public UpdateService<UpdateModel> UpdateService { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
        private ILogger<ReceiveCancel> Logger { get; set; }



        protected override async Task OnInitializedAsync()
        {
            receivevm = new List<ReceiveViewModel>();
            pageUrlList = new List<PageUrl>
            {
                new PageUrl("receive/abort/", "Abort", "oi-trash", "btn-outline-danger ml-2"),
                new PageUrl(null, "Approve", "oi-check", "btn-outline-info ml-2")
            };

            UpdateService.OnUpdateRequested += PageRefreshHandler;

            await LoadData(PagingParameter.CurrentPage);
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Id != Guid.Empty)
            {
                await Abort(Id);
            }
        }

        private async void PageRefreshHandler(string property, UpdateModel model)
        {
            await InvokeAsync(async () =>
            {
                if (property != null)
                {
                    foreach (var load in property.Split(new char[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (load == "toCancel")
                        {
                            await LoadData(PagingParameter.CurrentPage);
                            StateHasChanged();
                        }
                    }
                }

            });
        }

        private async Task LoadData(int page)
        {
            if (!isLock)
            {
                while (isLock)
                {
                    Logger.LogInformation("System is busy ...");
                    await Task.Delay(100);
                }

                await CallData(page);
            }

            PagingParameter.TotalPages = ReceiveService.PageCount();
            if (PagingParameter.TotalPages == 0)
            {
                spinnerOnOff = false;
            }

            if (receivevm.Count == 0 && PagingParameter.CurrentPage != 1)
            {
                PagingParameter.CurrentPage -= 1;
                if (!isLock)
                    await CallData(PagingParameter.CurrentPage);
            }
        }

        //EventCallback for paging navigation linkmo
        public async Task SelectedPage(int page)
        {
            PagingParameter.CurrentPage = page;
            await LoadData(PagingParameter.CurrentPage);
        }

        //Delete action for the remove request from user to receive item
        private async Task Delete(Guid id)
        {
            var receive = await ReceiveService.GetByIdAsync(id);
            var stock = await StockService.GetByIdAsync(receive.ProductId);
            int rQty = receive.Quantity - receive.UseQuantity;

            var parameters = new ModalParameters();
            parameters.Add(nameof(DeleteConfirmModal.Message),
                $"{((receive.Product.Prefix != null) ? receive.Product.Prefix.Name : "")} {receive.Product.Name}");

            var options = new ModalOptions { HideCloseButton = true, DisableBackgroundCancel = true, };
            var fromMessage = Modal.Show<DeleteConfirmModal>("Do you want to delete this?", parameters, options);

            var result = await fromMessage.Result;
            if (!result.Cancelled)
            {
                if (receive.UseQuantity == 0)
                {
                    stock.TotalReceive -= receive.Quantity;
                    int receiveQty = stock.TotalReceive - stock.TotalReceiveReturn;
                    int issueQty = stock.TotalIssue - stock.TotalIssueReturn;
                    stock.InStock = receiveQty - issueQty;
                    receive.IsDelete = true;
                }
                else
                {
                    //To manage stock
                    //int qty = receive.Quantity - receive.UseQuantity;
                    stock.TotalReceive -= rQty;
                    stock.InStock -= rQty;

                    //Create New Receive Item for deletion of available quentity in slipt manner
                    var newReceive = new Receive
                    {
                        ProductId = receive.ProductId,
                        ManufactureDate = receive.ManufactureDate,
                        ExpiryDate = receive.ExpiryDate,
                        ReceiveDate = receive.ReceiveDate,
                        ReceiveBy = receive.ReceiveBy,
                        Quantity = rQty,
                        Batch = receive.Batch,
                        IsDelete = true,
                        Note = receive.Note,
                        Remarks = receive.ReceiveBy,
                    };

                    await ReceiveService.CreateAsync(newReceive);

                    //receive manage for old entry
                    receive.Note = null;
                    receive.Quantity = receive.UseQuantity;
                    receive.UseQuantity = 0;
                    receive.IsUse = true;
                }

                await ReceiveService.UpdateAsync(receive);

                var isUpdate = await StockService.UpdateAsync(stock);

                if (isUpdate != null)
                {
                    Logger.LogInformation($"{isUpdate.Product.Name} {AlertMessage.DeleteInfo}");
                    AlertService.AddMessage(new Alert($"{isUpdate.Product.Name} removed successfully!", AlertType.Info));
                    await LoadData(PagingParameter.CurrentPage);
                    receive.Quantity = rQty; //Replace with cancel qty
                    UpdateService.UpdatePage(property: "cancel", entity: new UpdateModel { Receive = receive });
                }
            }
        }

        private async Task CallData(int page)
        {
            isLock = true;
            receivevm.Clear();
            await foreach (var receive in ReceiveService.StreamListAsync(
                page,
                PagingParameter.PageSize,
                filter: x => x.IsDelete != true && x.Note != null && x.IsUse != true,
                orderBy: o => o.OrderByDescending(x => x.ReceiveDate)
                    .ThenBy(x => x.Product.Prefix.Name)
                    .ThenBy(x => x.Product.Name),
                includeProperties: "Product,Product.Prefix,Product.Suffix"))
            {
                var vm = new ReceiveViewModel
                {
                    Id = receive.Id,
                    Name = $"{receive.Product.Prefix?.Name} {receive.Product.Name} " +
                        $"{receive.Product.Suffix?.Name}",
                    ReceiveDate = receive.ReceiveDate.ToShortDateString(),
                    Qty = receive.Quantity,
                    ReceiveBy = receive.ReceiveBy,
                    LifeSpan = receive.ExpiryDate.Subtract(DateTime.Now.Date).Days,
                    IsUsed = receive.IsUse,
                    Note = receive.Note,
                };

                receivevm.Add(vm);
                spinnerOnOff = false;
                StateHasChanged();
            }

            isLock = false;
        }

        private async Task Abort(Guid id)
        {
            NavigationManager.NavigateTo("/receive/abort", false);
            title = "Receive-abort";
            var item = await ReceiveService.GetByIdAsync(id);

            item.Note = null;
            var result = await ReceiveService.UpdateAsync(item);

            if (result != null)
            {
                Logger.LogInformation("Task completed!");
                AlertService.AddMessage(new Alert("Task completed successfully!", AlertType.Info));
                await LoadData(PagingParameter.CurrentPage);

                UpdateService.UpdatePage(property: "cancel");
            }
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageRefreshHandler;
        }
    }
}