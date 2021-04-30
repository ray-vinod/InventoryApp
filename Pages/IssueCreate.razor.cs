using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class IssueCreate : IDisposable
    {
        public IEnumerable<Product> products;
        public Issue issue;
        public Receive receive;
        public int avQty, oldQuantity;
        public string oldRemarks, oldUser;
        public bool IsDisable = false;
        public string title = "Issue-create";

        [Parameter]
        public Guid Id { get; set; }

        [Inject]
        private UpdateService<UpdateModel> UpdateService { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
        private AlertService AlertService { get; set; }
        [Inject]
        private IssueService IssueService { get; set; }
        [Inject]
        private ProductService ProductService { get; set; }
        [Inject]
        private StockService StockService { get; set; }
        [Inject]
        private SaleReturnService SaleReturnService { get; set; }
        [Inject]
        private ReceiveService ReceiveService { get; set; }
        [Inject]
        private ILogger<IssueCreate> Logger { get; set; }
        [Inject]
        public UserStateService UserStateService { get; set; }


        Product ProductSelection
        {
            get => issue.Product;
            set
            {
                issue.Product = value;
                if (value != null)
                    ChangeHandler(value).ConfigureAwait(false);
                StateHasChanged();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            issue = new Issue();

            UpdateService.OnUpdateRequested += PageUpdateHandler;

            products = await ProductService.GetItemsAsync(orderBy: o => o.OrderBy(x => x.Name),
                includeProperties: "Prefix,Suffix");

            ReadUserName();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Id != Guid.Empty)
            {
                await Return();
                IsDisable = true;
            }
        }

        //Product list auto refresh
        private async void PageUpdateHandler(string property, UpdateModel model)
        {
            await InvokeAsync(async () =>
            {
                products = await ProductService.GetItemsAsync(includeProperties:
                    "Product,Product.Prefix,Produc.Suffix");
                StateHasChanged();
            });
        }

        private void ReadUserName()
        {
            var result = UserStateService.Name;
            {
                issue.IssueBy = result;
            }

            if (issue.Remarks == null)
            {
                issue.Remarks = "From Pharmacy";
            }
        }

        private async Task Add()
        {
            string msg = issue.Product.Name;

            if (IsDisable)
            {
                //Return issue Item
                if (avQty >= issue.Quantity)
                {
                    var stock = await StockService.GetByIdAsync(issue.ProductId);

                    receive = await ReceiveService.GetByIdAsync(issue.PurchaseId);

                    //new sale return
                    var saleReturn = new SaleReturn
                    {
                        ProductId = issue.ProductId,
                        Quantity = issue.Quantity,
                        ReturnDate = issue.IssueDate,
                        Remarks = issue.Remarks.ToUpper(),
                        ReturneBy = issue.IssueBy,
                        IssueId = issue.Id,
                    };

                    //create new sale retun
                    await SaleReturnService.CreateAsync(saleReturn);
                    Logger.LogInformation("Sale return requested");

                    //Stock Update
                    stock.TotalIssueReturn += issue.Quantity;
                    stock.InStock = (stock.TotalReceive - stock.TotalReceiveReturn) -
                                    (stock.TotalIssue - stock.TotalIssueReturn);
                    await StockService.UpdateAsync(stock);
                    Logger.LogInformation("Stock updated");


                    //Receive update on sale return
                    if (receive.IsUse == true)
                    {
                        receive.IsUse = false;
                        receive.UseQuantity = receive.Quantity - issue.Quantity;
                    }
                    else
                    {
                        receive.UseQuantity -= issue.Quantity;
                    }

                    await ReceiveService.UpdateAsync(receive);
                    Logger.LogInformation("Receive table updated");


                    //Issue
                    issue.ReturnQty += issue.Quantity; //set returned qty
                    issue.Quantity = oldQuantity; //then set up back to normal issued qty
                    issue.Remarks = oldRemarks;
                    issue.IssueBy = oldUser;

                    if (issue.Quantity == issue.ReturnQty)
                    {
                        issue.IsUse = true;
                        issue.ReturnQty = 0;
                    }

                    var updatedIssue = await IssueService.UpdateAsync(issue);

                    if (updatedIssue != null)
                    {
                        Logger.LogInformation("Sale returned successfully!");
                        AlertService.AddMessage(new Alert(msg + "Sale returned successfully!",
                            AlertType.Info));

                        stock = new Stock();
                        issue = new Issue();
                        receive = new Receive();

                        UpdateService.UpdatePage();
                        //Redict to Issue List Page 
                        NavigationManager.NavigateTo("/issue/index", false);
                    }
                }
                else
                {
                    AlertService.AddMessage(new Alert("Quantity is not available as you want!",
                        AlertType.Error));
                    return;
                }
            }
            else
            {
                //Add Issue Item
                if (issue.Product == null && issue.Quantity == 0)
                {
                    return;
                }

                issue.Remarks = issue.Remarks.ToUpper();
                issue.ProductId = issue.Product.Id;
                issue.Product = null;

                //Stock management long running task
                bool save = await Task.Run(async () => await StockService.IssueItem(issue));
                if (save)
                {
                    Logger.LogInformation("sale item created!");
                    AlertService.AddMessage(new Alert(msg + AlertMessage.AddInfo, AlertType.Info));

                    UpdateService.UpdatePage();

                    //Reset Form
                    IsDisable = false;
                    issue = new Issue();
                    ReadUserName();
                }
            }
        }

        //From Search function
        public async Task<IEnumerable<Product>> SearchAsync(string searchText)
        {
            return await Task.FromResult(products.Where(p => p.Name.Contains(searchText.ToUpper())));
        }

        private async Task ChangeHandler(Product product)
        {
            var stockProduct = await StockService.GetByIdAsync(product.Id);

            if (stockProduct == null)
            {
                AlertService.AddMessage(new Alert("Item not found in stock!", AlertType.Warning));
                return;
            }

            avQty = stockProduct.InStock;
            oldQuantity = issue.Quantity;
            if (avQty == 0)
            {
                AlertService.AddMessage(new Alert("Item not available in stock!", AlertType.Warning));
                return;
            }
        }

        private async Task Return()
        {
            issue = await Task.Run(async () => await IssueService.GetByIdAsync(Id));
            avQty = issue.Quantity - issue.ReturnQty;
            oldRemarks = issue.Remarks;
            oldUser = issue.IssueBy;
            oldQuantity = issue.Quantity;
            issue.Remarks = "To Pharmacy";
            issue.Quantity = avQty;
            NavigationManager.NavigateTo("/issue/return", false);
            title = "Issue-return";
        }

        private void Reset()
        {
            IsDisable = false;
            issue.Quantity = oldQuantity;
            issue = new Issue();
            ReadUserName();
            NavigationManager.NavigateTo("/issue/create", false);
        }

        public void Dispose()
        {
            UpdateService.OnUpdateRequested -= PageUpdateHandler;
        }
    }
}